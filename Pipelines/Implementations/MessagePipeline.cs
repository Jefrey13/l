using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Hubs;
using CustomerService.API.Models;
using CustomerService.API.Pipelines.Interfaces;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;
using Mapster;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WhatsappBusiness.CloudApi.Messages.Requests;

namespace CustomerService.API.Pipelines.Implementations
{
    public class MessagePipeline : IMessagePipeline
    {
        private readonly IContactLogService _contactService;
        private readonly IConversationService _conversationService;
        private readonly IMessageService _messageService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly INotificationService _notification;
        private readonly IUserService _userService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IGeminiClient _geminiClient;
        private readonly INicDatetime _nicDatetime;
        private readonly string _systemPrompt;
        private readonly ISignalRNotifyService _signalR;
        private readonly IUnitOfWork _uow;

        private const int BotUserId = 1;

        public MessagePipeline(
            IContactLogService contactService,
            IConversationService conversationService,
            IMessageService messageService,
            IWhatsAppService whatsAppService,
            INotificationService notification,
            IUserService userService,
            IHubContext<ChatHub> hubContext,
            IGeminiClient geminiClient,
            INicDatetime nicDatetime,
            IUnitOfWork uow,
            ISignalRNotifyService signalR,
            IOptions<GeminiOptions> geminiOpts)
        {
            _contactService = contactService;
            _conversationService = conversationService;
            _messageService = messageService;
            _whatsAppService = whatsAppService;
            _notification = notification;
            _userService = userService;
            _hubContext = hubContext;
            _geminiClient = geminiClient;
            _nicDatetime = nicDatetime;
            _systemPrompt = geminiOpts.Value.SystemPrompt;
            _uow = uow;
            _signalR = signalR;
        }

        public async Task ProcessIncomingAsync(ChangeValue value, CancellationToken ct = default)
        {
            // 1. Normalizar payload
            var msg = value.Messages.First();
            var payload = new IncomingPayload
            {
                From = msg.From,
                TextBody = msg.Text?.Body,
                InteractiveId = msg.Interactive?.ListReply?.Id,
                Type = (msg.Interactive?.ListReply != null)
                                  ? InteractiveType.Interactive
                                  : InteractiveType.Text
            };

            // 2. Obtener o crear contacto
            var contactDto = await _contactService
                .GetOrCreateByPhoneAsync(payload.From, ct);
            var contactId = contactDto.Id;

            // 3. Obtener o crear conversación
            var convoDto = await _conversationService
                .GetOrCreateAsync(contactId, ct);

            var incoming = new Message
            {
                ConversationId = convoDto.ConversationId,
                SenderContactId = contactDto.Id,
                Content = msg.Text?.Body,
                //Por defecto por el momento es texto.
                MessageType = MessageType.Text,
                SentAt = DateTimeOffset.UtcNow,
                Status = MessageStatus.Delivered
            };
            await _uow.Messages.AddAsync(incoming, ct);
            await _uow.SaveChangesAsync(ct);

            var dto = incoming.Adapt<MessageDto>();
            await _signalR.NotifyUserAsync(
                 convoDto.ConversationId,
                "ReceiveMessage",
                dto);

            // 4. Si no inicializada, enviar saludo + menú interactivo
            if (!convoDto.Initialized)
            {
                await _conversationService.UpdateAsync(new UpdateConversationRequest
                {
                    ConversationId = convoDto.ConversationId,
                    Initialized = true
                }, ct);

                // Saludo
                var sendDto0 = new SendMessageRequest
                {
                    ConversationId = convoDto.ConversationId,
                    SenderId = BotUserId,
                    Content = "Hola, soy *Sofía*, tu asistente virtual de PC GROUP S.A. ¿En qué puedo ayudarte?",
                    MessageType = MessageType.Text,
                    File = null,
                    Caption = null
                };

                await _messageService.SendMessageAsync(sendDto0, ct);

                // Menú interactivo
                var buttons = new[] {
                    new WhatsAppInteractiveButton { Id = "1", Title = "Seguir con asistente" },
                    new WhatsAppInteractiveButton {Id = "2", Title = "Hablar con soporte" }
                };
                await _whatsAppService.SendInteractiveButtonsAsync(
                    convoDto.ConversationId, BotUserId,
                    "Selecciona una opción:", buttons, ct);

                return;
            }

            // 5. Si es respuesta interactiva...
            if (payload.Type == InteractiveType.Interactive)
            {
                switch (payload.InteractiveId)
                {
                    case "1":
                        // Continuar con bot
                        await _conversationService.UpdateAsync(new UpdateConversationRequest
                        {
                            ConversationId = convoDto.ConversationId,
                            Status = ConversationStatus.Bot
                        }, ct);

                        //await _whatsAppService.SendTextAsync(
                        //    convoDto.ConversationId, BotUserId,
                        //    "Perfecto, continuemos. ¿En qué más puedo ayudarte?", ct);

                        var sendDto = new SendMessageRequest
                        {
                            ConversationId = convoDto.ConversationId,
                            SenderId = BotUserId,
                            Content = "Perfecto, continuemos. ¿En qué más puedo ayudarte?",
                            MessageType = MessageType.Text,
                            File = null,
                            Caption = null
                        };

                        await _messageService.SendMessageAsync(sendDto, ct);
                        return;

                    case "2":
                        // Solicitar soporte humano
                        await _conversationService.UpdateAsync(new UpdateConversationRequest
                        {
                            ConversationId = convoDto.ConversationId,
                            Status = ConversationStatus.Waiting
                        }, ct);

                        // Mensaje de confirmación al cliente
                        //await _whatsAppService.SendTextAsync(
                        //    convoDto.ConversationId, BotUserId,
                        //    "Tu solicitud ha sido recibida. En breve un agente te atenderá.", ct);

                        var sendDto1 = new SendMessageRequest
                        {
                            ConversationId = convoDto.ConversationId,
                            SenderId = BotUserId,
                            Content = "Tu solicitud ha sido recibida. En breve un agente te atenderá.",
                            MessageType = MessageType.Text,
                            File = null,
                            Caption = null
                        };

                        await _messageService.SendMessageAsync(sendDto1, ct);


                        // Notificar a todos los admins
                        var admins = await _userService
                            .GetByRoleAsync("Admin", ct); // devuelve List<UserDto>

                        var adminIds = admins.Select(a => a.UserId).ToArray();

                        var supportPayload = JsonSerializer.Serialize(new
                        {
                            convoDto.ConversationId,
                            contactDto.Phone,
                            contactDto.WaName
                        });

                        await _notification.CreateAsync(
                            NotificationType.SupportRequested,
                            supportPayload,
                            adminIds,
                            ct);

                        return;
                }
            }

            // 6. Estado BOT: IA + respuesta
            if (convoDto.Status == ConversationStatus.Bot)
            {
                // ... tu lógica Gemini y respuesta ...
                await HandleBotReplyAsync(convoDto, payload.TextBody, ct);
                return;
            }

            // 7. Estado Waiting/Human: persistir mensaje y notificar por SignalR
            await _messageService.SendMessageAsync(new SendMessageRequest
            {
                ConversationId = convoDto.ConversationId,
                SenderId = contactId,
                Content = payload.TextBody
            }, ct);

            //await _hubContext.Clients
            //    .Group(convoDto.ConversationId.ToString())
            //    .SendAsync("ReceiveMessage", sentDto, ct);
        }

        private async Task HandleBotReplyAsync(
            ConversationDto convoDto,
            string? userText,
            CancellationToken ct = default)
        {
            // 1. Recuperar el histórico de la conversación
            var history = await _messageService
                .GetByConversationAsync(convoDto.ConversationId, ct);

            // 2. Construir el prompt para Gemini
            var allTexts = history
                .Select(m => m.Content)
                .Where(t => !string.IsNullOrWhiteSpace(t));

            var fullPrompt = _systemPrompt
                           + Environment.NewLine
                           + string.Join(Environment.NewLine, allTexts)
                           + Environment.NewLine
                           + (userText ?? "");

            // 3. Invocar Gemini
            var botReply = (await _geminiClient
                .GenerateContentAsync(fullPrompt, userText ?? "", ct))
                .Trim();

            // 4. Enviar y persistir el mensaje del bot
            var sendReq = new SendMessageRequest
            {
                ConversationId = convoDto.ConversationId,
                SenderId = BotUserId,
                Content = botReply
            };
            await _messageService.SendMessageAsync(sendReq, ct);
            // (MessageService internamente guarda en BD, envía por WhatsApp API y dispara SignalR)
        }

    }
}