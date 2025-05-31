using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
using Newtonsoft.Json.Linq;
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
        private readonly IGeminiClient _geminiClient;

        public ConversationService(
            IUnitOfWork uow,
            INicDatetime nicDatetime,
            INotificationService notification,
            IHubContext<ChatHub> hubContext,
            ITokenService tokenService,
            IGeminiClient geminiClient)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _nicDatetime = nicDatetime ?? throw new ArgumentNullException(nameof(nicDatetime));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
            _hubContext = hubContext;
            _tokenService = tokenService;
            _geminiClient = geminiClient;
        }

        public async Task<IEnumerable<ConversationDto>> GetAllAsync(CancellationToken cancellation = default)
        {
            var convs = await _uow.Conversations.GetAll()
                .Include(c => c.Messages)
                    .ThenInclude(a=>a.Attachments)
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

        public async Task AssignAgentAsync(int conversationId, int agentUserId, string status, string jwtToken, CancellationToken ct = default)
        {
            try
            {

            if (conversationId <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));
            
            var conv = await _uow.Conversations.GetByIdAsync(conversationId, ct)
                ?? throw new KeyNotFoundException("Conversation not found.");


            var principal = _tokenService.GetPrincipalFromToken(jwtToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            conv.AssignedAgentId = agentUserId;
            conv.AssignedByUserId = userId;
            conv.AssignedAt = await _nicDatetime.GetNicDatetime();

            conv.Status = ConversationStatus.Human;
            _uow.Conversations.Update(conv);
            await _uow.SaveChangesAsync(ct);

            
            
            await _notification.CreateAsync(
                NotificationType.ConversationAssigned,
                "Se te ha asignado la conversación",
                new[] { agentUserId },   // aquí va solo el id del agente
                ct);

            conv = await _uow.Conversations.GetByIdAsync(conversationId, ct)
               ?? throw new KeyNotFoundException("Conversation not found.");

            var dto = conv.Adapt<ConversationDto>();

            dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;

            await _hubContext.Clients
             .Group("Admin")
             .SendAsync("ConversationUpdated", dto, ct);

            // al usuario de Support al que se le asigno la conversación
            await _hubContext.Clients
                .User(agentUserId.ToString())
                .SendAsync("ConversationUpdated", dto, ct);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<ConversationDto?> GetByIdAsync(int id, CancellationToken cancellation = default)
        {
            try
            {
                if (id <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(id));

                var conv = await _uow.Conversations.GetByIdAsync(id, CancellationToken.None);

                return conv == null ? null : conv.Adapt<ConversationDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ConversationDto();
            }
        }

        public async Task CloseAsync(int conversationId, CancellationToken ct = default)
        {
            if (conversationId <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));
            var conv = await _uow.Conversations.GetByIdAsync(conversationId, ct)
                ?? throw new KeyNotFoundException("Conversation not found.");

            conv.Status = ConversationStatus.Closed;
            conv.ClosedAt = await _nicDatetime.GetNicDatetime();
            _uow.Conversations.Update(conv);
            await _uow.SaveChangesAsync(ct);

            var updatedConv = await _uow.Conversations.GetByIdAsync(conv.ConversationId);

            var dto = updatedConv.Adapt<ConversationDto>();

            dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;

            await _hubContext
            .Clients
            .All
            .SendAsync("ConversationUpdated", dto, ct);
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

            //await _hubContext
            //    .Clients
            //    .All
            //    .SendAsync("ConversationCreated", dto, cancellation);
            await _hubContext.Clients.Group("Admin")
                .SendAsync("ConversationCreated", dto, cancellation);


            return dto;
        }

        public async Task UpdateAsync(UpdateConversationRequest request, CancellationToken ct = default)
        {

            try
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

            dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;
            await _hubContext
                .Clients
                .All
                .SendAsync("ConversationUpdated", dto, ct);
        }

        public async Task<IEnumerable<ConversationHistoryDto>> GetHistoryByContactAsync(int convId, CancellationToken ct = default)
        {
            try
            {
                var conv = await _uow.Conversations.GetByIdAsync(convId, ct) ?? new Conversation();

                var convs = await _uow.Conversations.GetAll()
                    .Where(c => c.ClientContact.Id == conv.ClientContactId)
                    .Include(cl => cl.ClientContact)
                    .Include(cl => cl.AssignedAgent)
                    .Include(cl => cl.AssignedByUser)
                    .Include(c => c.Messages)
                        .ThenInclude(s => s.SenderUser)
                    .Include(c => c.Messages)
                        .ThenInclude(s => s.SenderContact)
                    .Include(c => c.Messages)
                        .ThenInclude(m => m.Attachments)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync(ct);

                var convDto = convs.Select(c => new ConversationHistoryDto
                {
                    ConversationId = c.ConversationId,
                    CreatedAt = c.CreatedAt,
                    Status = c.Status!.Value,
                    Messages = c.Messages
                                         .OrderBy(m => m.SentAt)
                                         .Select(m => new MessageWithAttachmentsDto
                                         {
                                             MessageId = m.MessageId,
                                             SenderUserId = m.SenderUserId,
                                             SenderUserName = m.SenderUser?.FullName,
                                             SenderContactId = m.SenderContactId,
                                             SenderContactName =m.SenderContact?.WaName,
                                             Content = m.Content,
                                             SentAt = m.SentAt,
                                             MessageType = m.MessageType,
                                             Attachments = m.Attachments
                                                                    .Select(a => a.Adapt<AttachmentDto>())
                                         })
                });

                return convDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el historial de conversaciones: {ex.Message}");
                return Enumerable.Empty<ConversationHistoryDto>();
            }
        }

        public async Task<string> SummarizeAllByContactAsync(int contactId, CancellationToken ct = default)
        {
            // 1) Traer el historial completo con mensajes y attachments
            var history = await GetHistoryByContactAsync(contactId, ct);

            // 2) Construir el texto plano
            var sb = new StringBuilder();
            foreach (var conv in history)
            {
                sb.AppendLine($"--- Conversación #{conv.ConversationId} ({conv.CreatedAt:g}) ---");
                foreach (var msg in conv.Messages)
                {
                    var who = msg.SenderUserId.HasValue ? "Agente" : "Cliente";
                    sb.AppendLine($"{who}: {msg.Content}");
                    foreach (var att in msg.Attachments)
                    {
                        sb.AppendLine($"   📎 {att.FileName} ({att.MimeType})");
                    }
                }
            }

            // 3) Invocar a Gemini para resumir
            var prompt = "Resume este histórico completo en un párrafo breve, de los mensajes enviados por el clintes. Es para ver el resumen de sus mensajes:";
            return await _geminiClient.GenerateContentAsync(prompt, sb.ToString(), ct);
        }
    }
}