using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.RequestDtos.ConversationDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Hubs;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;
using Humanizer;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WhatsappBusiness.CloudApi.Messages.Requests;
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
        private readonly IHubContext<NotificationsHub> _hubNotification;
        private readonly ITokenService _tokenService;
        private readonly IGeminiClient _geminiClient;
        private readonly IMessageService _messageService;
        private readonly ISystemParamService _systemParamService;

        public ConversationService(
            IUnitOfWork uow,
            INicDatetime nicDatetime,
            INotificationService notification,
            IHubContext<ChatHub> hubContext,
            IHubContext<NotificationsHub> hubNotification,
            ITokenService tokenService,
            IGeminiClient geminiClient,
            IMessageService messageService,
            ISystemParamService systemParamService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _nicDatetime = nicDatetime ?? throw new ArgumentNullException(nameof(nicDatetime));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
            _hubContext = hubContext;
            _tokenService = tokenService;
            _geminiClient = geminiClient;
            _hubNotification = hubNotification ?? throw new ArgumentException(nameof(_hubNotification));
            _messageService = messageService;
            _systemParamService = systemParamService ?? throw new ArgumentException(nameof(systemParamService));
        }

        //public async Task<IEnumerable<ConversationResponseDto>> GetAllAsync(CancellationToken cancellation = default)
        //{
        //    var convs = await _uow.Conversations.GetAll()
        //        .Include(c => c.Messages)
        //            .ThenInclude(a => a.Attachments)
        //        .Include(c => c.ClientContact)
        //        .Include(c => c.AssignedAgent)
        //        .Include(c => c.AssignedByUser)
        //        .ToListAsync(cancellation);

        //    var dto = convs.Select(c => c.Adapt<ConversationResponseDto>());

        //    foreach (var item in dto)
        //    {
        //        var tone = await GetToneStringAsync(item.ConversationId);
        //        item.Tone = tone;
        //    }

        //    return dto;
        //}

        public async Task<IEnumerable<ConversationResponseDto>> GetAllAsync(CancellationToken cancellation = default)
        {
            var convs = await _uow.Conversations.GetAll()
                .Include(c => c.Messages)
                    .ThenInclude(a => a.Attachments)
                .Include(c => c.ClientContact)
                .Include(c => c.AssignedAgent)
                .Include(c => c.AssignedByUser)
                .ToListAsync(cancellation);

            var dtoList = convs
                .Select(c => c.Adapt<ConversationResponseDto>())
                .ToList();

            //foreach (var item in dtoList)
            //{
            //    item.Tone = await GetToneStringAsync(item.ConversationId);
            //}

            return dtoList;
        }


        public async Task<IEnumerable<ConversationResponseDto>> GetPendingAsync(CancellationToken cancellation = default)
        {
            var convs = await _uow.Conversations.GetAll()
                .Where(c => c.Status == ConversationStatus.Waiting || c.Status == ConversationStatus.Bot)
                .Include(c => c.Messages)
                .Include(c => c.ClientContact)
                .Include(c => c.AssignedAgent)
                .Include(c => c.AssignedByUser)
                .ToListAsync(cancellation);

            var dto = convs.Adapt<ConversationResponseDto>();

            dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;

            return convs.Select(c => c.Adapt<ConversationResponseDto>());
        }
        public async Task<ConversationResponseDto> StartAsync(StartConversationRequest request, CancellationToken cancellation = default)
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


            var dto = conv.Adapt<ConversationResponseDto>();

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

                var localDate = await _nicDatetime.GetNicDatetime();
                conv.AssignedAgentId = agentUserId;
                conv.AssignedByUserId = userId;
                //conv.AssignedAt = await _nicDatetime.GetNicDatetime();
                conv.AssignmentState = AssignmentState.Pending;
                conv.RequestedAgentAt = await _nicDatetime.GetNicDatetime();
                conv.Status = ConversationStatus.Waiting;

                _uow.Conversations.Update(conv);
                await _uow.SaveChangesAsync(ct);

                var updatedConv = await _uow.Conversations.GetByIdAsync(conv.ConversationId);


                //await _notification.CreateAsync(
                //    NotificationType.ConversationAssigned,
                //    $"Nueva conversación asiganda a: {updatedConv?.AssignedAgent?.FullName}",
                //    new[] { agentUserId },   // aquí va solo el id del agente
                //    ct);

                //await _hubContext.Clients
                // .User(agentUserId.ToString())
                // .SendAsync("AssignmentRequested", new
                // {
                //     ConversationId = conv.ConversationId,
                //     RequestedAt = conv.RequestedAgentAt
                // }, ct);



                //conv = await _uow.Conversations.GetByIdAsync(conversationId, ct)
                //    ?? throw new KeyNotFoundException("Conversation not found.");


                //var updateConv = _uow.Conversations.GetByIdAsync(updatedConv.ConversationId);

                var dto = updatedConv.Adapt<ConversationResponseDto>();

                //await _hubContext
                //       .Clients
                //       .All
                //       .SendAsync("ConversationUpdated", updatedConv, ct);



                dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;

                //await _hubContext.Clients
                // .Group("Admin")
                // .SendAsync("ConversationUpdated", dto, ct);

                // al usuario de Support al que se le asigno la conversación
                //await _hubContext.Clients
                //.User(agentUserId.ToString())
                //.SendAsync("ConversationUpdated", dto, ct);

                //await _hubContext.Clients
                //.User(agentUserId.ToString())
                //.SendAsync("AssignmentRequested", dto, ct);

                //await _hubContext.Clients
                //   .User("Admin")
                //   .SendAsync("AssignmentRequested", dto, ct);

                await _messageService.SendMessageAsync(new SendMessageRequest
                {
                    ConversationId = dto.ConversationId,
                    SenderId = 1,
                    Content = $"Su solicitud esta siendo procesada, por favor espere un momento.",
                    MessageType = MessageType.Text
                }, false, ct);

                //await _hubContext.Clients
                //   .User(agentUserId.ToString())
                //   .SendAsync("ConversationUpdated", dto, ct);

                // 3) emítelo por SignalR
                //if (dto.Status == ConversationStatus.Human.ToString())
                //{
                //    await _hubContext.Clients
                //      .All
                //      .SendAsync("ConversationUpdated", dto, ct);
                //}
                //else
                //{
                //    await _hubContext.Clients
                //    .Group("Admin")
                //    .SendAsync("ConversationUpdated", dto, ct);
                //}
                await _hubContext.Clients
                 .Group("Admin")
                 .SendAsync("ConversationUpdated", dto, ct);

                await _hubNotification
                      .Clients
                      .Group(agentUserId.ToString())   // o .User(agentUserId.ToString())
                      .SendAsync("ConversationAssigned", dto, ct);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task RespondAssignmentAsync(int conversationId, bool accepted, string? comment, CancellationToken ct)
        {
            try
            {

                var conv = await _uow.Conversations.GetByIdAsync(conversationId, ct)
                           ?? throw new KeyNotFoundException("Conversación no encontrada");

                conv.Justification = comment;
                conv.AssignmentState = accepted
                                            ? AssignmentState.Accepted
                                            : AssignmentState.Rejected;

                if (accepted)
                {
                    conv.AssignedAt = await _nicDatetime.GetNicDatetime();
                    conv.Status = ConversationStatus.Human;
                    conv.AssignmentState = AssignmentState.Accepted;

                    await _messageService.SendMessageAsync(new SendMessageRequest
                    {
                        ConversationId = conv.ConversationId,
                        SenderId = 1,
                        Content = $"Se te asigno un nuevo agente: {conv.AssignedAgent?.FullName}. Ahora puedes comunicarte con el.",
                        MessageType = MessageType.Text
                    }, false, ct);
                }

                _uow.Conversations.Update(conv, ct);
                await _uow.SaveChangesAsync(ct);

                var updatedConv = await _uow.Conversations.GetByIdAsync(conv.ConversationId);
                var dto = updatedConv.Adapt<ConversationResponseDto>();

                if (dto.Status == ConversationStatus.Waiting.ToString())
                {
                    await _hubContext.Clients
                                .Group("Admin")
                                .SendAsync("ConversationUpdated", updatedConv, ct);
                }
                else if (dto.Status == ConversationStatus.Human.ToString())
                {
                    await _hubContext.Clients
                                .Group("Admin")
                                .SendAsync("ConversationUpdated", dto, ct);

                    await _hubContext.Clients
                       .User(dto.AssignedAgentId.ToString())
                       .SendAsync("ConversationUpdated", dto, ct);

                }

                // Notificar al admin
                await _hubNotification.Clients
                     .Group("Admins")
                     .SendAsync("AssignmentResponse", new
                     {
                         dto.ConversationId,
                         dto.AssignedAgentName,
                         accepted,
                         comment,
                         dto.Justification,
                     }, ct);

                Console.WriteLine("Datos del support es: ", dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errores", ex.Message);
            }
        }

        public async Task ForceAssignAsync(int conversationId, bool forced, string? assignmentComment, CancellationToken ct)
        {
            try
            {

                var conv = await _uow.Conversations.GetByIdAsync(conversationId, ct)
                           ?? throw new KeyNotFoundException("Conversación no encontrada");

                if (forced)
                {
                    //conv.AssignedAgentId = targetAgentId;
                    //conv.AssignedByUserId = 1;
                    conv.AssignedAt = await _nicDatetime.GetNicDatetime();
                    //conv.AssignmentResponseAt = conv.AssignedAt;
                    conv.AssignmentComment = assignmentComment;
                    conv.AssignmentState = AssignmentState.Forced;
                    conv.Status = ConversationStatus.Human;

                    _uow.Conversations.Update(conv, ct);
                    await _uow.SaveChangesAsync(ct);

                    var updatedConv = await _uow.Conversations.GetByIdAsync(conv.ConversationId);
                    var dto = updatedConv.Adapt<ConversationResponseDto>();

                    await _messageService.SendMessageAsync(new SendMessageRequest
                    {
                        ConversationId = dto.ConversationId,
                        SenderId = 1,
                        Content = $"Se te asigno un nuevo agente: {dto.AssignedAgentName}. Ahora puedes comunicarte con el.",
                        MessageType = MessageType.Text
                    }, false, ct);
                }
                else
                {
                    // limpiar campos si rechazó
                    conv.AssignedAgentId = null;
                    conv.AssignedByUserId = null;
                    conv.AssignmentState = AssignmentState.Rejected;
                    conv.Status = ConversationStatus.Waiting;

                    _uow.Conversations.Update(conv, ct);
                    await _uow.SaveChangesAsync(ct);
                }

                var updatedConv2 = await _uow.Conversations.GetByIdAsync(conv.ConversationId);
                var dto2 = updatedConv2.Adapt<ConversationResponseDto>();

                // Notificar tanto al agente forzado como a los admins
                if (dto2.Status == ConversationStatus.Human.ToString())
                {
                    await _hubNotification.Clients
                        .User(conv.AssignedAgentId.ToString())
                        .SendAsync("AssignmentForced", new
                        {
                            conv.ConversationId,
                            conv.AssignmentComment
                        }, ct);
                }

                //await _hubNotification.Clients
                //    .Group("Admins")
                //    .SendAsync("AssignmentForcedAdmin", new
                //    {
                //        conv.ConversationId
                //    }, ct);

                await _hubContext.Clients
                                .Group("Admin")
                                .SendAsync("ConversationUpdated", dto2, ct);

                await _hubContext.Clients
               .User(dto2.AssignedAgentId.ToString())
               .SendAsync("ConversationUpdated", dto2, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //public async Task<ConversationResponseDto?> GetByIdAsync(int id, CancellationToken cancellation = default)
        //{
        //    try
        //    {
        //        if (id <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(id));

        //        var conv = await _uow.Conversations.GetByIdAsync(id, CancellationToken.None);

        //        var tone = await GetToneStringAsync(conv.ConversationId);
        //        conv.Tone = tone;

        //        _uow.Conversations.Update(conv, cancellation);
        //        await _uow.SaveChangesAsync(cancellation);

        //        var convUpdated = await _uow.Conversations.GetByIdAsync(id, CancellationToken.None);

        //        return conv == null ? null : convUpdated.Adapt<ConversationResponseDto>();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //        return new ConversationResponseDto();
        //    }
        //}

        public async Task<ConversationResponseDto?> GetByIdAsync(int id, CancellationToken cancellation = default)
        {
            try
            {
                if (id <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(id));

                var conv = await _uow.Conversations.GetByIdAsync(id, CancellationToken.None);

                return conv == null ? null : conv.Adapt<ConversationResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ConversationResponseDto();
            }
        }

        public async Task<IEnumerable<ConversationStatusCountResponseDto>> GetConversationsCountByDateRange(FilterDashboard filters, CancellationToken ct = default)
        {
            //if (from == null && to == null) throw new  ArgumentException("Date range parameters are reguired");

            var convs = await _uow.Conversations.GetConversationsCountByDateRange(filters, ct);

            if (convs == null) throw new ArgumentException("Date range parameters are reguired");

            return convs;
        }
        public async Task<PagedResponse<ConversationResponseDto>> GetByState(
            PaginationParams @params, string state, CancellationToken ct = default)
        {
            try
            {
                var query = _uow.Conversations
                    .QueryByState(state)
                    .OrderBy(c => c.CreatedAt);

                var paged = await PagedList<Conversation>
                    .CreateAsync(query, @params.PageNumber, @params.PageSize, ct);

                var dtos = paged.Select(sp => sp.Adapt<ConversationResponseDto>());

                return new PagedResponse<ConversationResponseDto>(dtos, paged.MetaData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task<IEnumerable<WaitingClientResponseDto>> GetWaitingClient(FilterDashboard filters, int? criticalMinutes, CancellationToken ct = default)
        {
            var data = await _uow.Conversations.GetWaitingClient(filters, criticalMinutes, ct);

            if (data == null) throw new NullReferenceException("Data not found");

            return data;
        }

        public async Task<ResponseAgentAverageResponseDto> ResponseAgentAverageAsync(FilterDashboard filters, CancellationToken ct = default)
        {
            var data = await _uow.Conversations.ResponseAgentAverageAsync(filters, ct);

            if (data == null) throw new NullReferenceException("Data not found");

            return data;
        }


        public async Task<IEnumerable<AdminAsigmentResponseTimeResponseDto>> AssigmentResponseTimeAsync(FilterDashboard filters, CancellationToken ct = default)
        {
            var data = await _uow.Conversations.AssigmentResponseTimeAsync(filters, ct);

            if (data == null) throw new Exception("Date not found");

            return data;
        }
        public async Task<IEnumerable<AverageAssignmentTimeResponseDto>> AverageAssignmentTimeAsync(FilterDashboard filters, CancellationToken ct = default)
        {
            var agent = await _uow.Conversations.AverageAssignmentTimeAsync(filters, ct);

            if (agent == null) throw new ArgumentException("Date not found");

            return agent;
        }

        public async Task CloseAsync(int conversationId, CancellationToken ct = default)
        {
            try
            {
                if (conversationId <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(conversationId));

                var conv = await _uow.Conversations.GetByIdAsync(conversationId, ct)
                    ?? throw new KeyNotFoundException("Conversation not found.");

                conv.Status = ConversationStatus.Closed;
                conv.ClosedAt = await _nicDatetime.GetNicDatetime();
                _uow.Conversations.Update(conv);
                await _uow.SaveChangesAsync(ct);

                var updatedConv = await _uow.Conversations.GetByIdAsync(conv.ConversationId);

                var dto = updatedConv.Adapt<ConversationResponseDto>();

                dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;

                bool isAgentAssign = dto.Status == ConversationStatus.Human.ToString() ? true : false;

                await _messageService.SendMessageAsync(new SendMessageRequest
                {
                    ConversationId = dto.ConversationId,
                    SenderId = 1,
                    Content = $"Su conversación {(isAgentAssign
                                    ? $"con el agente: {dto.AssignedAgentName} "
                                    : string.Empty)}finalizó. Muchas gracias por su tiempo.",

                    MessageType = MessageType.Text
                }, false, ct);

                //await _hubContext
                //    .Clients
                //    .All
                //    .SendAsync("ConversationUpdated", dto, ct);

                await _hubContext.Clients
                                .Group("Admin")
                                .SendAsync("ConversationUpdated", dto, ct);

                if (dto.AssignedAgentId != null)
                {
                    await _hubContext.Clients
                   .User(dto.AssignedAgentId.ToString())
                   .SendAsync("ConversationUpdated", dto, ct);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<ConversationResponseDto> GetOrCreateAsync(
            int clientContactId,
            CancellationToken ct = default)
        {
            try
            {
                if (clientContactId <= 0)
                    throw new ArgumentException("Invalid contact ID.", nameof(clientContactId));

                var conv = await _uow.Conversations.GetAll()
                    .Where(c => c.ClientContactId == clientContactId
                             && c.Status != ConversationStatus.Closed
                             && c.Status != ConversationStatus.Incomplete)
                    .SingleOrDefaultAsync();

                //Si tiene conversación en estado activo se obtiene
                if (conv != null)
                    return conv.Adapt<ConversationResponseDto>();

                //Si no hay conversaciones activas (closed o incomplete) se crea una nueva.
                var contact = await _uow.ContactLogs.GetByIdAsync(clientContactId, ct)
                              ?? throw new KeyNotFoundException($"Contact {clientContactId} not found.");

                var localDate = await _nicDatetime.GetNicDatetime();
                conv = new Conversation
                {
                    ClientContactId = clientContactId,
                    Status = ConversationStatus.Bot,
                    CreatedAt = localDate,
                    Initialized = false,
                    AssignedByUserId = 2,
                    ClientFirstMessage = localDate,
                };

                await _uow.Conversations.AddAsync(conv, ct);
                await _uow.SaveChangesAsync(ct);

                //var full = await _uow.Conversations.GetAll()
                //    .Where(c => c.ConversationId == conv.ConversationId)
                //    .Include(c => c.Messages)m
                //    .SingleAsync(cancellation);

                var dto = conv.Adapt<ConversationResponseDto>();

                dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;

                //await _hubContext
                //    .Clients
                //    .All
                //    .SendAsync("ConversationCreated", dto, cancellation);
                //await _hubContext.Clients.Group("Admin")
                //    .SendAsync("ConversationCreated", dto, cancellation)

                return dto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ConversationResponseDto();
            }
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

                if (request.RequestedAgentAt.HasValue)
                    conv.RequestedAgentAt = request.RequestedAgentAt.Value;

                if (request.Tags != null)
                    conv.Tags = request.Tags;

                var localTime = await _nicDatetime.GetNicDatetime();

                conv.UpdatedAt = localTime;

                if (request.Status == ConversationStatus.Waiting) conv.AgentRequestAt = localTime;

                _uow.Conversations.Update(conv, ct);

                await _uow.SaveChangesAsync(ct);

                var dto = conv.Adapt<ConversationResponseDto>();

                dto.TotalMessages = dto.TotalMessages == 0 ? 3 : dto.TotalMessages + 2;


                if (dto.Status == ConversationStatus.Waiting.ToString())
                {
                    await _hubContext
                     .Clients
                     .Group("Admin")
                     .SendAsync("ConversationUpdated", dto, ct);
                }
                else
                {
                    await _hubContext
                     .Clients
                     .All
                     .SendAsync("ConversationUpdated", dto, ct);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<ConversationResponseDto> UpdateConversationAssignmentStateAsync(UpdateConversationRequestDto updateConversationRequestDto, CancellationToken cancellation = default)
        {
            if (updateConversationRequestDto.ConversationId <= 0)
                throw new ArgumentException("Invalid conversation ID.", nameof(updateConversationRequestDto.ConversationId));

            var conv = await _uow.Conversations.GetByIdAsync(updateConversationRequestDto.ConversationId, cancellation)

                ?? throw new KeyNotFoundException($"Conversation {updateConversationRequestDto.ConversationId} not found.");

            // Actualizar el estado de asignación
            conv.AssignmentState = updateConversationRequestDto.Status ?? conv.AssignmentState;
            conv.Justification = updateConversationRequestDto.Justification ?? conv.Justification;
            conv.UpdatedAt = await _nicDatetime.GetNicDatetime();

            //Su el usuario rechaza la asignación, se limpia de asignació pendiente que habia.
            if (updateConversationRequestDto.Status == AssignmentState.Rejected)
            {
                conv.AssignedAgent = null;
                conv.AssignedAgentId = null;
                conv.AssignedAt = null;
                conv.AssignedByUser = null;
                conv.AssignedByUserId = null;
            }

            _uow.Conversations.Update(conv, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            var updatedConv = _uow.Conversations.GetByIdAsync(updateConversationRequestDto.ConversationId, cancellation)
                ?? throw new KeyNotFoundException($"Conversation {updateConversationRequestDto.ConversationId} not found.");

            var dto = updatedConv.Adapt<ConversationResponseDto>();

            //Notificar via sinalR a todos los clientes
            await _hubContext.Clients.Group("Admin")
                .SendAsync("Conversationassigned", dto, cancellation);

            return dto;
        }

        public async Task<int> GetAssignedCountAsync(int agentUserId, CancellationToken cancellation = default)
        {
            return await _uow.Conversations.CountAssignedAsync(agentUserId, cancellation);

        }

        /// <summary>
        /// GEt conversation by role and add tone of conversatiio. This could be a little costly
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ConversationResponseDto>> GetConversationByRole(string jwtToken, CancellationToken cancellation = default)
        {
            var principal = _tokenService.GetPrincipalFromToken(jwtToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var roles = principal.FindAll(ClaimTypes.Role).Select(r => r.Value);

            if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                return await GetAllAsync(cancellation);

            var convs = await _uow.Conversations.GetByAgentAsync(userId, cancellation);

           var dto = convs.Select(c => c.Adapt<ConversationResponseDto>());

            //foreach (var item in dto)
            //{
            //    var tone = await GetToneStringAsync(item.ConversationId);
            //    item.Tone = tone;
            //}

            return dto;
        }

        public async Task UpdateTags(int id, List<string> request, CancellationToken ct = default)
        {
            if (request.Count <= 0) throw new ArgumentException("Por favor pasar las tags.");

            var conv = await _uow.Conversations.GetByIdAsync(id);

            conv.Tags = request;
            conv.UpdatedAt = await _nicDatetime.GetNicDatetime();

            _uow.Conversations.Update(conv, ct);

            await _uow.SaveChangesAsync();

            var dto = conv.Adapt<ConversationResponseDto>();

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
                                         .Select(m => new MessageWithAttachmentsResponseDto
                                         {
                                             MessageId = m.MessageId,
                                             SenderUserId = m.SenderUserId,
                                             SenderUserName = m.SenderUser?.FullName,
                                             SenderContactId = m.SenderContactId,
                                             SenderContactName = m.SenderContact?.WaName,
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
        public async Task AutoAssingAsync(int convId, int agentUserId, CancellationToken ct = default)
        {
            try
            {
                if (convId <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(convId));

                var conv = await _uow.Conversations.GetByIdAsync(convId, ct)
                    ?? throw new KeyNotFoundException("Conversation not found.");

                var localDate = await _nicDatetime.GetNicDatetime();
                conv.AssignedAgentId = agentUserId;
                conv.AssignedByUserId = 1;
                conv.AssignedAt = await _nicDatetime.GetNicDatetime();
                conv.AssignmentState = AssignmentState.Forced;
                conv.RequestedAgentAt = await _nicDatetime.GetNicDatetime();
                conv.Status = ConversationStatus.Human;

                _uow.Conversations.Update(conv);
                await _uow.SaveChangesAsync(ct);

                var updatedConv = await _uow.Conversations.GetByIdAsync(conv.ConversationId);

                var dto = updatedConv.Adapt<ConversationResponseDto>();

                await _messageService.SendMessageAsync(new SendMessageRequest
                {
                    ConversationId = conv.ConversationId,
                    SenderId = 1,
                    Content = $"Se le asigno el agente de turno {dto.AssignedAgentName}, ya puede conversar con el 💬.",
                    MessageType = MessageType.Text
                }, false, ct);

                var updated = await _uow.Conversations.GetByIdAsync(conv.ConversationId);

                // Notificar tanto al agente forzado como a los admins
                if (updated.Status.ToString() == ConversationStatus.Human.ToString())
                {
                    await _hubNotification.Clients
                        .User(updated.AssignedAgentId.ToString())
                        .SendAsync("AssignmentForced", new
                        {
                            updated.ConversationId,
                            updated.AssignmentComment
                        }, ct);
                }

                await _hubContext.Clients
                     .Group("Admin")
                     .SendAsync("ConversationUpdated", updated, ct);

                await _hubContext.Clients
                   .User(updated.AssignedAgentId.ToString())
                   .SendAsync("ConversationUpdated", updated, ct);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public async Task UpdateState(int convId, ConversationStatus conversationStatus, CancellationToken ct = default)
        {
            var entity = await _uow.Conversations.GetByIdAsync(convId, ct);

            entity.Status = conversationStatus;

            _uow.Conversations.Update(entity);
            await _uow.SaveChangesAsync(ct);
        }
        public async Task MarkConversationReadAsync(int conversationId, string jwtToken, CancellationToken ct = default)
        {
            var conv = await _uow.Conversations.GetByIdAsync(conversationId, ct);
            var principal = _tokenService.GetPrincipalFromToken(jwtToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var lastClientMsgId = conv.Messages.Max(m => (int?)m.MessageId) ?? 0;


            if (conv.AssignedAgentId == userId)
            {
                conv.AgentLastReadMessageId = lastClientMsgId;
            }
            else if (conv.AssignedByUserId == userId)
            {
                conv.AssignerLastReadMessageId = lastClientMsgId;
            }

            _uow.Conversations.Update(conv);
            await _uow.SaveChangesAsync(ct);

            var updateDto = conv.Adapt<ConversationResponseDto>();

            await _hubContext.Clients
               .User(userId.ToString())
               .SendAsync("ConversationUpdated", updateDto, ct);
        }

        public async Task<string> GetToneStringAsync(int ConversationId, CancellationToken ct = default)
        {
            try
            {
                if (ConversationId <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(ConversationId));

                var allMessages = (await _messageService
                    .GetByConversationAsync(ConversationId, ct))
                    .Select(m => m.Content)
                    .Where(t => !string.IsNullOrWhiteSpace(t));

                var fullPrompt = $@"
                        Historial de mensajes:
                        {string.Join('\n', allMessages)}";

                var botReply = (await _geminiClient
                    .GenerateContentAsync(fullPrompt, "Aliza el tono de los mensajes de la conversación en base a la siguiente escala " +
                    "Los valores de 1 al 5 representan a: " +
                    "5 = Muy deficiente: Mensajes confusos, faltos de coherencia o cortesía.\r\n\r\n " +
                    "4 = Deficiente: Algunas mensajes se entienden, pero hay problemas de claridad o tono.\r\n\r\n " +
                    "3 = Aceptable: Los mensajes no presentan faltas de respeto ni ideas confusas.\r\n\r\n " +
                    "2 = Bueno: Mensajes claros, coherentes y respetuosos; cumple su objetivo de atención al cliente responsable..\r\n\r\n" +
                    "1 = Excelente: Muy bien estructurada, empatía en el tono, enfoque preciso y sin errores." +
                    "Retorna como respues un valor entre 1 a 5 en base a tu analisis sobre que valor se le deve de asignar a la conversación de atención al cliente. No retornes otra respuesta que sea siempre un numero entre 1 a 5."
                    , ct))
                    .Trim();

                var conversationTone = "No disponible";

                if (botReply == null)
                {
                    throw new InvalidOperationException("Invalid tone value returned by Gemini.");
                }

                switch (botReply)
                {
                    case var _ when botReply.Contains("1", StringComparison.OrdinalIgnoreCase):
                        conversationTone = "Excelente"; // Positivo
                        break;
                    case var _ when botReply.Contains("2", StringComparison.OrdinalIgnoreCase):
                        conversationTone = "Deficiente"; // Neutral
                        break;
                    case var _ when botReply.Contains("3", StringComparison.OrdinalIgnoreCase):
                        conversationTone = "Aceptable"; // Negativo
                        break;
                    case var _ when botReply.Contains("4", StringComparison.OrdinalIgnoreCase):
                        conversationTone = "Deficiente"; // Muy positivo
                        break;
                    case var _ when botReply.Contains("5", StringComparison.OrdinalIgnoreCase):
                        conversationTone = "Muy deficiente"; // Muy negativo
                        break;
                }

                return conversationTone;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "No disponible";
            }
        }
        public async Task<int> GetToneAsync(int ConversationId, CancellationToken ct = default)
        {
            try
            {
                if (ConversationId <= 0) throw new ArgumentException("Invalid conversation ID.", nameof(ConversationId));

                var allMessages = (await _messageService
                    .GetByConversationAsync(ConversationId, ct))
                    .Select(m => m.Content)
                    .Where(t => !string.IsNullOrWhiteSpace(t));

                var fullPrompt = $@"
                        Historial de mensajes:
                        {string.Join('\n', allMessages)}";

                var botReply = (await _geminiClient
                    .GenerateContentAsync(fullPrompt, "Aliza el tono de los mensajes de la conversación en base a la siguiente escala " +
                    "Los valores de 1 al 5 representan a: " +
                    "5 = Muy deficiente: Mensajes confusos, faltos de coherencia o cortesía.\r\n\r\n " +
                    "4 = Deficiente: Algunas mensajes se entienden, pero hay problemas de claridad o tono.\r\n\r\n " +
                    "3 = Aceptable: Los mensajes no presentan faltas de respeto ni ideas confusas.\r\n\r\n " +
                    "2 = Bueno: Mensajes claros, coherentes y respetuosos; cumple su objetivo de atención al cliente responsable..\r\n\r\n" +
                    "1 = Excelente: Muy bien estructurada, empatía en el tono, enfoque preciso y sin errores." +
                    "Retorna como respues un valor entre 1 a 5 en base a tu analisis sobre que valor se le deve de asignar a la conversación de atención al cliente. No retornes otra respuesta que sea siempre un numero entre 1 a 5."
                    , ct))
                    .Trim();

                var conversationTone = 1;

                if (conversationTone < 1 || conversationTone > 5 && !(conversationTone is int))
                {
                    //throw new InvalidOperationException("Invalid tone value returned by Gemini.");
                }

                switch (botReply)
                {
                    case var _ when botReply.Contains("1", StringComparison.OrdinalIgnoreCase):
                        conversationTone = 1; // Positivo
                        break;
                    case var _ when botReply.Contains("2", StringComparison.OrdinalIgnoreCase):
                        conversationTone = 2; // Neutral
                        break;
                    case var _ when botReply.Contains("3", StringComparison.OrdinalIgnoreCase):
                        conversationTone = 3; // Negativo
                        break;
                    case var _ when botReply.Contains("4", StringComparison.OrdinalIgnoreCase):
                        conversationTone = 4; // Muy positivo
                        break;
                    case var _ when botReply.Contains("5", StringComparison.OrdinalIgnoreCase):
                        conversationTone = 5; // Muy negativo
                        break;
                }

                return conversationTone;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        public async Task<PagedResponse<ConversationResponseDto>> GetConversationByClient(PaginationParams @params, FilterDashboard filters,
            CancellationToken ct = default)
        {
            if (filters.Equals(null)) 
                return new PagedResponse<ConversationResponseDto>(null, null);

            try
            {
                var query = _uow.Conversations
                    .GetConversationByClient(filters, ct);

                var paged = await PagedList<Conversation>
                    .CreateAsync(query, @params.PageNumber, @params.PageSize, ct);

                var dtos = paged.Select(sp => sp.Adapt<ConversationResponseDto>());

                return new PagedResponse<ConversationResponseDto>(dtos, paged.MetaData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}

//Muy deficiente: Mensajes confusos, faltos de coherencia o cortesía.

//Deficiente: Algunas ideas se entienden, pero hay problemas de claridad o tono.

//Aceptable: Se transmite el mensaje principal, con pequeños fallos de estilo o detalle.

//Bueno: Clara, coherente y respetuosa; cumple su objetivo con soltura.

//Excelente: Muy bien estructurada, empatía en el tono, enfoque preciso y sin errores.