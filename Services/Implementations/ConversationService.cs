using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Hubs;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WhatsappBusiness.CloudApi.Webhook;
using Conversation = CustomerService.API.Models.Conversation;

namespace CustomerService.API.Services.Implementations
{
    public class ConversationService : IConversationService
    {
        private readonly IUnitOfWork _uow;
        private readonly INicDatetime _nicDatetime;
        private readonly INotificationService _notification;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ITokenService _tokenService;

        public ConversationService(
            IUnitOfWork uow,
            INicDatetime nicDatetime,
            INotificationService notification,
            IHubContext<ChatHub> hubContext,
            ITokenService tokenService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _nicDatetime = nicDatetime ?? throw new ArgumentNullException(nameof(nicDatetime));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
            _hubContext = hubContext;
            _tokenService = tokenService;
        }

        public async Task<IEnumerable<ConversationDto>> GetAllAsync(CancellationToken cancellation = default)
        {
            var convs = await _uow.Conversations.GetAll()
                .Include(c => c.Messages)
                .Include(c => c.ClientContact)
                .Include(c => c.AssignedAgent)
                .Include(c => c.AssignedByUser)
                .ToListAsync(cancellation);

            var dto = convs.Adapt<ConversationDto>();

            dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;

            return convs.Select(c => c.Adapt<ConversationDto>());
        }

        public async Task<IEnumerable<ConversationDto>> GetPendingAsync(CancellationToken cancellation = default)
        {
            var convs = await _uow.Conversations.GetAll()
                .Where(c => c.Status == ConversationStatus.Waiting || c.Status == ConversationStatus.Bot)
                .Include(c => c.Messages)
                .Include(c => c.ClientContact)
                .Include(c => c.AssignedAgent)
                .Include(c => c.AssignedByUser)
                .ToListAsync(cancellation);

            var dto = convs.Adapt<ConversationDto>();

            dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;

            return convs.Select(c => c.Adapt<ConversationDto>());
        }
        public async Task<ConversationDto> StartAsync(StartConversationRequest request, CancellationToken cancellation = default)
        {
            if (request.CompanyId <= 0)
                throw new ArgumentException("CompanyId must be greater than zero.", nameof(request.CompanyId));
            if (request.ClientContactId <= 0)
                throw new ArgumentException("ClientContactId must be greater than zero.", nameof(request.ClientContactId));

            var now = await _nicDatetime.GetNicDatetime();
            var conv = new Conversation
            {
                //CompanyId = request.CompanyId,
                ClientContactId = request.ClientContactId,
                Priority = request.Priority,
                Status = ConversationStatus.Bot,
                CreatedAt = now,
                FirstResponseAt = null,
                Tags = request.Tags ?? new List<string>()
            };

            await _uow.Conversations.AddAsync(conv, cancellation);
            await _uow.SaveChangesAsync(cancellation);


            var dto = conv.Adapt<ConversationDto>();

            dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;

            await _hubContext
                .Clients
                .All
                .SendAsync("ConversationCreated", dto, cancellation);

            return dto;

        }

        public async Task AssignAgentAsync(int conversationId, int agentUserId, string status, CancellationToken cancellation = default)
        {
            if (conversationId <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));
            var conv = await _uow.Conversations.GetByIdAsync(conversationId, cancellation)
                ?? throw new KeyNotFoundException("Conversation not found.");

            conv.AssignedAgentId = agentUserId;
            conv.AssignedByUserId = null; // o setear desde contexto.
            conv.AssignedAt = await _nicDatetime.GetNicDatetime();

            conv.Status = ConversationStatus.Human;
            _uow.Conversations.Update(conv);
            await _uow.SaveChangesAsync(cancellation);

            var payload = JsonSerializer.Serialize(new { conv.ConversationId, Agent = agentUserId });
            
            //await _notification.CreateAsync(
            //    NotificationType.ConversationAssigned,
            //    payload,
            //    new[] { agentUserId },
            //    cancellation);
            
            var dto = conv.Adapt<ConversationDto>();

            dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;

            await _hubContext
            .Clients
            .All
            .SendAsync("ConversationUpdated", dto, cancellation);
        }

        public async Task<ConversationDto?> GetByIdAsync(int id, CancellationToken cancellation = default)
        {
            if (id <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(id));
            var conv = await _uow.Conversations.GetAll()
                .Include(c => c.Messages)
                .Include(c => c.ClientContact)
                .Include(c => c.AssignedAgent)
                .Include(c => c.AssignedByUser)
                .SingleOrDefaultAsync(c => c.ConversationId == id, cancellation);

            return conv == null ? null : conv.Adapt<ConversationDto>();
        }

        public async Task CloseAsync(int conversationId, CancellationToken cancellation = default)
        {
            if (conversationId <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));
            var conv = await _uow.Conversations.GetByIdAsync(conversationId, cancellation)
                ?? throw new KeyNotFoundException("Conversation not found.");

            conv.Status = ConversationStatus.Closed;
            conv.ClosedAt = await _nicDatetime.GetNicDatetime();
            _uow.Conversations.Update(conv);
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task<ConversationDto> GetOrCreateAsync(
            int clientContactId,
            CancellationToken cancellation = default)
        {
            if (clientContactId <= 0)
                throw new ArgumentException("Invalid contact ID.", nameof(clientContactId));


            var conv = await _uow.Conversations.GetAll()
                .Where(c => c.ClientContactId == clientContactId
                         && c.Status != ConversationStatus.Closed)
                .Include(c => c.Messages)
                .SingleOrDefaultAsync(cancellation);

            if (conv != null)
                return conv.Adapt<ConversationDto>();

            var contact = await _uow.ContactLogs.GetByIdAsync(clientContactId, cancellation)
                          ?? throw new KeyNotFoundException($"Contact {clientContactId} not found.");

            var now = await _nicDatetime.GetNicDatetime();
            conv = new Conversation
            {
                ClientContactId = clientContactId,
                Status = ConversationStatus.Bot,
                CreatedAt = now,
                Initialized = false
            };

            await _uow.Conversations.AddAsync(conv, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            //var full = await _uow.Conversations.GetAll()
            //    .Where(c => c.ConversationId == conv.ConversationId)
            //    .Include(c => c.Messages)m
            //    .SingleAsync(cancellation);

            var dto = conv.Adapt<ConversationDto>();

            dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;

            await _hubContext
                .Clients
                .All
                .SendAsync("ConversationCreated", dto, cancellation);

            return dto;
        }

        public async Task UpdateAsync(UpdateConversationRequest request, CancellationToken ct = default)
        {
            if (request.ConversationId <= 0)
                throw new ArgumentException("Invalid conversation ID.", nameof(request.ConversationId));

            var conv = await _uow.Conversations
                .GetAll()
                .SingleOrDefaultAsync(c => c.ConversationId == request.ConversationId, ct)
                ?? throw new KeyNotFoundException($"Conversation {request.ConversationId} not found.");

            // Actualizar campos opcionales
            if (request.Priority.HasValue)
                     conv.Priority = request.Priority.Value;
            if (request.Initialized.HasValue)
                conv.Initialized = request.Initialized.Value;
            if (request.Status.HasValue)
                conv.Status = request.Status.Value;
            if (request.AssignedAgentId.HasValue)
                conv.AssignedAgentId = request.AssignedAgentId;
            if (request.IsArchived.HasValue)
                conv.IsArchived = request.IsArchived.Value;

            if (request.Tags != null)
                conv.Tags = request.Tags;

            try
            {
                conv.UpdatedAt = await _nicDatetime.GetNicDatetime();


                _uow.Conversations.Update(conv, ct);

                await _uow.SaveChangesAsync(ct);

                var dto = conv.Adapt<ConversationDto>();

                dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;


                await _hubContext
                     .Clients
                     .All
                     .SendAsync("ConversationUpdated", dto, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public async Task<int> GetAssignedCountAsync(int agentUserId, CancellationToken cancellation = default)
        {
            return await _uow.Conversations.CountAssignedAsync(agentUserId, cancellation);

        }

        public async Task<IEnumerable<ConversationDto>> GetConversationByRole(string jwtToken, CancellationToken cancellation = default)
        {
            var principal = _tokenService.GetPrincipalFromToken(jwtToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var roles = principal.FindAll(ClaimTypes.Role).Select(r => r.Value);

            if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                return await GetAllAsync(cancellation);

            var convs = await _uow.Conversations.GetByAgentAsync(userId, cancellation);

            return convs.Select(c => c.Adapt<ConversationDto>());
        }

        public async Task UpdateTags(int id, List<string> request, CancellationToken ct = default)
        {
            if (request.Count <= 0) throw new ArgumentException("Por favor pasar las tags.");

            var conv = await _uow.Conversations.GetByIdAsync(id);

            conv.Tags = request;
            conv.UpdatedAt = await _nicDatetime.GetNicDatetime();

            _uow.Conversations.Update(conv, ct);

            await _uow.SaveChangesAsync();

            var dto = conv.Adapt<ConversationDto>();

            await _hubContext
                .Clients
                .All
                .SendAsync("ConversationUpdated", dto, ct);
        }
    }
}