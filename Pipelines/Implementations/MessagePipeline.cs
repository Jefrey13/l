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
            private readonly IAttachmentService _attachmentService;

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
                IOptions<GeminiOptions> geminiOpts,
                IAttachmentService attachmentService)
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
                _attachmentService = attachmentService;
        }

            public async Task ProcessIncomingAsync(ChangeValue value, CancellationToken ct = default)
            {
                var msg = value.Messages.First();

                var payload = new IncomingPayload
                {
                    From = msg.From,
                    TextBody = msg.Text?.Body,
                    InteractiveId = msg.Interactive?.ListReply?.Id,
                    Type = msg.Interactive?.ListReply != null
                                    ? InteractiveType.Interactive
                                    : InteractiveType.Text
                };


            var contactDto = await _contactService.GetOrCreateByPhoneAsync(
                    payload.From,
                    value.Contacts.First().WaId,
                    value.Contacts.First().Profile.Name,
                    value.Contacts.First().UserId,
                    ct);

                var convoDto = await _conversationService.GetOrCreateAsync(contactDto.Id, ct);


            // 2) Si traemos media (imagen, video, audio, sticker, documento)…
            if (msg.Image != null
             || msg.Video != null
             || msg.Audio != null
             || msg.Sticker != null
             || msg.Document != null)
            {
                // 2.1) Extraer mediaId, mimeType y caption (si existiera)
                string mediaId = msg.Image?.Id
                               ?? msg.Video?.Id
                               ?? msg.Audio?.Id
                               ?? msg.Sticker?.Id
                               ?? msg.Document?.Id!;
                
                string mimeType = msg.Image?.MimeType
                                ?? msg.Video?.MimeType
                                ?? msg.Audio?.MimeType
                                ?? msg.Sticker?.MimeType
                                ?? msg.Document!.MimeType;
                
                string? caption = msg.Caption;

                // 2.2) Descargar URL y luego el binario
                var downloadUrl = await _whatsAppService.DownloadMediaUrlAsync(mediaId, ct);
                var data = await _whatsAppService.DownloadMediaAsync(downloadUrl, ct);

                try
                {
                    //Save local storage
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media");
                    Directory.CreateDirectory(folder);
                    var ext = Path.GetExtension(downloadUrl) ?? ".bin";
                    var file = $"{mediaId}{ext}";
                    var path = Path.Combine(folder, file);
                    await File.WriteAllBytesAsync(path, data, ct);
                    var publicUrl = $"/media/{file}";
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                // 2.3) Persistir el mensaje en BD (isContact = true)
                var sendReq = new SendMessageRequest
                {
                    ConversationId = convoDto.ConversationId,
                    SenderId = contactDto.Id,
                    Content = caption,
                    MessageType =  mimeType.Equals("image/webp") ? MessageType.Sticker
                                   : mimeType.StartsWith("image/") ? MessageType.Image
                                   : mimeType.StartsWith("video/") ? MessageType.Video
                                   : mimeType.StartsWith("audio/") ? MessageType.Audio
                                   : MessageType.Document
                };

                var messageDto = await _messageService.SendMessageAsync(sendReq, isContact: true, CancellationToken.None);

                // 2.4) Guardar el attachment en BD
                var attachReq = new UploadAttachmentRequest
                {
                    MessageId = messageDto.MessageId,
                    MediaId = mediaId,
                    MimeType = mimeType,
                    FileName = msg.Document?.Filename ?? mediaId,
                    MediaUrl = downloadUrl,
                    // Opcional: FileSize = data.Length
                };
                await _attachmentService.UploadAsync(attachReq, CancellationToken.None);

                return;
            }

            if (payload.Type == InteractiveType.Text)
            {
                await _messageService.SendMessageAsync(new SendMessageRequest
                {
                    ConversationId = convoDto.ConversationId,
                    SenderId = contactDto.Id,
                    Content = payload.TextBody,
                    MessageType = MessageType.Text
                }, isContact: true, ct);
            }

            var convEntity = await _uow.Conversations.GetByIdAsync(convoDto.ConversationId, CancellationToken.None);

                    // 2) mapea a DTO
                    var convDto = convEntity.Adapt<ConversationDto>();

                    // 3) emítelo por SignalR
                    await _hubContext.Clients
                       .All
                       .SendAsync("ConversationUpdated", convDto, ct);

            var buttons = new[]
                {
                    new WhatsAppInteractiveButton { Id = "1", Title = "Seguir con asistente" },
                    new WhatsAppInteractiveButton { Id = "2", Title = "Hablar con soporte" }
                };

                if (!convoDto.Initialized)
                {
                _uow.ClearChangeTracker();

                await _conversationService.UpdateAsync(new UpdateConversationRequest
                    {
                        ConversationId = convoDto.ConversationId,
                        Initialized = true,
                        Priority = PriorityLevel.Normal,
                        Status = ConversationStatus.Bot,
                        IsArchived = false
                    }, ct);

                    await _messageService.SendMessageAsync(new SendMessageRequest
                    {
                        ConversationId = convoDto.ConversationId,
                        SenderId = BotUserId,
                        Content = "Hola, soy *Sofía*, tu asistente virtual de PC GROUP S.A. ¿En qué puedo ayudarte?",
                        MessageType = MessageType.Text
                    }, false, ct);

                    await _whatsAppService.SendInteractiveButtonsAsync(
                        convoDto.ConversationId,
                        BotUserId,
                        "Selecciona una opción:",
                        buttons,
                        ct
                    );

                      await _hubContext.Clients
                         .All
                         .SendAsync("ConversationUpdated", convDto, ct);


                return;
                }

                //Cuando el usuario seleccione una opcion de la lista enviada.
                if (payload.Type == InteractiveType.Interactive)
                {
                    var selected = buttons.FirstOrDefault(b => b.Id == payload.InteractiveId);
                    var title = selected?.Title ?? payload.InteractiveId;


                    //la opcion seleccionada por el cliente, por eso el true
                    await _messageService.SendMessageAsync(new SendMessageRequest
                    {
                        ConversationId = convoDto.ConversationId,
                        SenderId = contactDto.Id,
                        Content = title,
                        MessageType = MessageType.Interactive,
                        InteractiveId = payload.InteractiveId,
                        InteractiveTitle = title,

                    }, true, ct);

                            await _hubContext.Clients
                                .All
                                .SendAsync("ConversationUpdated", convDto, ct);

                switch (payload.InteractiveId)
                    {
                        case "1":
                            if(convDto.Status != ConversationStatus.Human.ToString())
                        {
                            await _conversationService.UpdateAsync(new UpdateConversationRequest
                            {
                                ConversationId = convoDto.ConversationId,
                                Status = ConversationStatus.Bot
                            }, ct);

                            await _messageService.SendMessageAsync(new SendMessageRequest
                            {
                                ConversationId = convoDto.ConversationId,
                                SenderId = BotUserId,
                                Content = "Perfecto, continuemos. ¿En qué más puedo ayudarte?",
                                MessageType = MessageType.Text
                            }, false, ct);

                            await _hubContext.Clients
                                .All
                                .SendAsync("ConversationUpdated", convDto, ct);
                        }
                        else
                        {
                            await _messageService.SendMessageAsync(new SendMessageRequest
                            {
                                ConversationId = convoDto.ConversationId,
                                SenderId = BotUserId,
                                Content = "Lo sentimos, su conversación esta procesada por un miembro de soporte, por el momento no puede comunicarse con el bot.",
                                MessageType = MessageType.Text
                            }, false, ct);

                            await _hubContext.Clients
                                .All
                                .SendAsync("ConversationUpdated", convDto, ct);
                        }
                        return;

                        case "2":
                            if(convDto.Status != ConversationStatus.Waiting.ToString())
                            {
                                await _conversationService.UpdateAsync(new UpdateConversationRequest
                                {
                                    ConversationId = convoDto.ConversationId,
                                    Status = ConversationStatus.Waiting
                                }, ct);

                                await _messageService.SendMessageAsync(new SendMessageRequest
                                {
                                    ConversationId = convoDto.ConversationId,
                                    SenderId = BotUserId,
                                    Content = "Tu solicitud ha sido recibida. En breve un agente te atenderá.",
                                    MessageType = MessageType.Text
                                }, false, ct);

                                await _hubContext.Clients
                                   .All
                                   .SendAsync("ConversationUpdated", convDto, ct);

                                var admins = await _userService.GetByRoleAsync("Admin", ct);
                                var adminIds = admins.Select(a => a.UserId).ToArray();


                                //Enviar una notificacion a los usuario del sitio web, por medio de signalr para mostrar una alerta con toast, y admeas actualizar el contandor en la opcion de notificaciones menu.
                                var supportJson = JsonSerializer.Serialize(new
                                {
                                    convoDto.ConversationId,
                                    contactDto.Phone,
                                    contactDto.WaName
                                });

                                await _notification.CreateAsync(
                                    NotificationType.SupportRequested,
                                    supportJson,
                                    adminIds,
                                    ct
                                );
                        }
                        else
                        {
                            await _messageService.SendMessageAsync(new SendMessageRequest
                            {
                                ConversationId = convoDto.ConversationId,
                                SenderId = BotUserId,
                                Content = "Lo sentimo su conversación esta siendo atendida por un agente de soporte, por el momento deve de finalizar ka conversación actual.",
                                MessageType = MessageType.Text
                            }, false, ct);

                            await _hubContext.Clients
                               .All
                               .SendAsync("ConversationUpdated", convDto, ct);
                        }

                            return;
                    }
                }

                if (convoDto.Status == ConversationStatus.Bot.ToString())
                {
                    await HandleBotReplyAsync(convoDto, payload.TextBody, ct);
                    return;
                }

                //await _messageService.SendMessageAsync(new SendMessageRequest
                //{
                //    ConversationId = convoDto.ConversationId,
                //    SenderId = contactDto.Id,
                //    Content = payload.TextBody
                //}, false , ct);
            }

            private async Task HandleBotReplyAsync(
                ConversationDto convoDto,
                string? userText,
                CancellationToken ct = default)
            {
                // Recuperar el histórico de la conversación
                var history = await _messageService
                    .GetByConversationAsync(convoDto.ConversationId, ct);

                // Construir el prompt para Gemini
                var allTexts = history
                    .Select(m => m.Content)
                    .Where(t => !string.IsNullOrWhiteSpace(t));

                var fullPrompt = _systemPrompt
                               + Environment.NewLine
                               + string.Join(Environment.NewLine, allTexts)
                               + Environment.NewLine
                               + (userText ?? "");

                // Invocar Gemini
                var botReply = (await _geminiClient
                    .GenerateContentAsync(fullPrompt, userText ?? "", ct))
                    .Trim();

                // Enviar y persistir el mensaje del bot
                var sendReq = new SendMessageRequest
                {
                    ConversationId = convoDto.ConversationId,
                    SenderId = BotUserId,
                    Content = botReply
                };
                await _messageService.SendMessageAsync(sendReq, false, ct);

            var convEntity = await _uow.Conversations.GetAll()
            .Where(c => c.ConversationId == convoDto.ConversationId)
            .Include(c => c.ClientContact)
            .SingleAsync(ct);

                        var convDto = convEntity.Adapt<ConversationDto>();

                        await _hubContext.Clients
                            .All
                            .SendAsync("ConversationUpdated", convDto, ct);
            // (MessageService internamente guarda en BD, envía por WhatsApp API y dispara SignalR)
        }

        }
    }