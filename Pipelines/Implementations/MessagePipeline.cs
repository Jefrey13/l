using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Hosted;
using CustomerService.API.Hubs;
using CustomerService.API.Models;
using CustomerService.API.Pipelines.Interfaces;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;
using CustomerService.API.WhContext;
using HtmlAgilityPack;
using Humanizer;
using Mapster;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using PuppeteerSharp;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WhatsappBusiness.CloudApi.Messages.Requests;
using WhatsappBusiness.CloudApi.Webhook;

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
        private readonly IHubContext<NotificationsHub> _hubNotification;
        private readonly IGeminiClient _geminiClient;
        private readonly INicDatetime _nicDatetime;
        private readonly string _systemPrompt;
        private readonly ISignalRNotifyService _signalR;
        private readonly IUnitOfWork _uow;
        private readonly IAttachmentService _attachmentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MessagePrompts _prompts;
        private readonly MessageKeywords _keywords;
        private readonly ISystemParamService _systemParamService;
        private readonly HttpClient _httpClient;
        private const int BotUserId = 1;
        private readonly IWebHostEnvironment _env;
        private readonly IOpeningHourService _openingHourService;
        private readonly IWorkShiftService _workShiftService;
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
            IAttachmentService attachmentService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<MessagePrompts> promptOpts,
            IOptions<MessageKeywords> keywordOpts,
            ISystemParamService systemParamService,
            IHubContext<NotificationsHub> hubNotification,
            HttpClient httpClient,
            IWebHostEnvironment env,
            IOpeningHourService openingHourService,
            IWorkShiftService workShiftService)
        {
            _contactService = contactService;
            _conversationService = conversationService;
            _messageService = messageService;
            _whatsAppService = whatsAppService;
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
            _userService = userService;
            _hubContext = hubContext;
            _geminiClient = geminiClient;
            _nicDatetime = nicDatetime;
            _systemPrompt = geminiOpts.Value.SystemPrompt;
            _uow = uow;
            _signalR = signalR;
            _attachmentService = attachmentService;
            _httpContextAccessor = httpContextAccessor;
            _prompts = promptOpts.Value;
            _keywords = keywordOpts.Value;
            _systemParamService = systemParamService;
            _hubNotification = hubNotification;
            _httpClient = httpClient;
            _env = env;
            _openingHourService = openingHourService;
            _workShiftService = workShiftService;
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

            var convDto = convEntity.Adapt<ConversationResponseDto>();

            // 3) emítelo por SignalR
            if (convDto.Status == ConversationStatus.Human.ToString())
            {
                await _hubContext.Clients
                  .All
                  .SendAsync("ConversationUpdated", convDto, ct);
            }
            else
            {
                await _hubContext.Clients
                    .Group("Admin")
                    .SendAsync("ConversationUpdated", convDto, ct);
            }

            //   PEDIR “FullName” si Status == New o AwaitingFullName

            var systemParam = await _systemParamService.GetAllAsync();
            try
            {

                if (contactDto.Status == ContactStatus.New
                 || contactDto.Status == ContactStatus.AwaitingFullName)
                {
                    // Si es NEW, enviamos la pregunta de nombre completo y cambiamos a AwaitingFullName
                    if (contactDto.Status == ContactStatus.New)
                    {
                        await _messageService.SendMessageAsync(
                            new SendMessageRequest
                            {
                                ConversationId = convoDto.ConversationId,
                                SenderId = BotUserId,
                                //Content = _prompts.AskFullName,    // “¿Cuál es tu nombre completo?”
                                Content = systemParam.FirstOrDefault(p => p.Name == "AskFullName")?.Value ?? "¿Cuál es tu nombre completo?",
                                MessageType = MessageType.Text
                            },
                            isContact: false,
                            ct);

                        await _contactService.UpdateContactDetailsAsync(new UpdateContactLogRequestDto
                        {
                            Id = contactDto.Id,
                            Status = ContactStatus.AwaitingFullName
                        }, ct);

                        var convEntity1 = await _uow.Conversations.GetByIdAsync(convoDto.ConversationId, CancellationToken.None);

                        var convDto1 = convEntity1.Adapt<ConversationResponseDto>();

                        // 3) emítelo por SignalR
                        await _hubContext.Clients
                           .All
                           .SendAsync("ConversationUpdated", convDto1, ct);

                        return;
                    }

                    // Si ya estaba en AwaitingFullName → recibimos el nombre en payload.TextBody
                    var nombre = payload.TextBody?.Trim() ?? "";

                    await _contactService.UpdateContactDetailsAsync(new UpdateContactLogRequestDto
                    {
                        Id = contactDto.Id,
                        FullName = nombre,
                        Status = ContactStatus.AwaitingIdCard
                    }, ct);

                    //var textoParaCedula = string.Format(_prompts.AskIdCard, nombre);
                    var textoParaCedula = string.Format(systemParam.FirstOrDefault(sp => sp.Name == "AskIdCard")?.Value ?? "¿Cual es su numero de cedula?", nombre);
                    
                    await _messageService.SendMessageAsync(
                    new SendMessageRequest
                    {
                        ConversationId = convoDto.ConversationId,
                        SenderId = BotUserId,
                        Content = textoParaCedula,   // “{Nombre}, por favor envía tu cédula (10 dígitos)”
                        MessageType = MessageType.Text
                    },
                    isContact: false,
                    ct);

                    var convEntity3 = await _uow.Conversations.GetByIdAsync(convoDto.ConversationId, CancellationToken.None);

                    var convDto3 = convEntity3.Adapt<ConversationResponseDto>();

                    // 3) emítelo por SignalR
                    await _hubContext.Clients
                       .All
                       .SendAsync("ConversationUpdated", convDto, ct);

                    return;
                }

                //   BLOQUE B: VALIDAR “IdCard” si Status == AwaitingIdCard

                if (contactDto.Status == ContactStatus.AwaitingIdCard)
                {
                    var cedula = payload.TextBody?.Trim() ?? "";

                    // Regex para 10 dígitos consecutivos (Nicaragua)
                    if (!Regex.IsMatch(cedula, @"^\d{3}-?\d{6}-?\d{4}[A-Za-z]$"))
                    {
                        await _messageService.SendMessageAsync(
                            new SendMessageRequest
                            {
                                ConversationId = convoDto.ConversationId,
                                SenderId = BotUserId,
                                //Content = _prompts.InvalidIdFormat, // “Formato inválido. Debe ser 10 dígitos.”
                                Content = systemParam.FirstOrDefault(p => p.Name == "InvalidIdFormat")?.Value ?? "Formato inválido. Debe ser formato valido.",
                                MessageType = MessageType.Text
                            },
                            isContact: false,
                            ct);

                        var convEntity4 = await _uow.Conversations.GetByIdAsync(convoDto.ConversationId, CancellationToken.None);

                        var convDto4 = convEntity4.Adapt<ConversationResponseDto>();

                        // 3) emítelo por SignalR
                        await _hubContext.Clients
                           .All
                           .SendAsync("ConversationUpdated", convDto, ct);

                        return;
                    }

                    // Si cédula válida, guardamos IdCard y cambiamos a Completed
                    await _contactService.UpdateContactDetailsAsync(new UpdateContactLogRequestDto
                    {
                        Id = contactDto.Id,
                        IdCard = cedula,
                        Status = ContactStatus.Completed
                    }, ct);

                    await _messageService.SendMessageAsync(
                        new SendMessageRequest
                        {
                            ConversationId = convoDto.ConversationId,
                            SenderId = BotUserId,
                            //Content = _prompts.DataComplete,
                            Content = systemParam.FirstOrDefault(p => p.Name == "DataComplete")?.Value ?? "Datos completos. ¿En qué puedo ayudarte?",
                            MessageType = MessageType.Text
                        },
                        isContact: false,
                        ct);

                    var convEntity5 = await _uow.Conversations.GetByIdAsync(convoDto.ConversationId, CancellationToken.None);

                    var convDto5 = convEntity.Adapt<ConversationResponseDto>();



                    // 3) emítelo por SignalR
                    await _hubContext.Clients
                       .All
                       .SendAsync("ConversationUpdated", convDto5, ct);

                    await _hubNotification
                      .Clients
                      .Group("Admins")
                      .SendAsync("newContactValidation", contactDto, ct);

                    // A partir de aquí, el pipeline continúa con el flujo normal
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Hola", ex);
            }

            // Solo aplicamos esta detección si el mensaje es texto y no es un payload interactivo:
            if (payload.Type == InteractiveType.Text && !string.IsNullOrWhiteSpace(payload.TextBody))
            {
                // Convertimos a minusculas para comparación
                var textoMinuscula = payload.TextBody.Trim().ToLower();

                // Revisar si alguna de las palabras clave está contenida

                //bool quiereSoporte = _keywords.Keywords
                //    .Any(kw => textoMinuscula.Contains(kw.ToLowerInvariant()));

                // bool quiereSoporte = systemParam.Any(p => p.Name == "Keywords" && p.Value.Split(',').Any(kw => textoMinuscula.Contains(kw.Trim().ToLowerInvariant())));

                //Estraer y comparar desde el json almacenado en la db.
                bool requestSupport = systemParam
                      .Where(p => p.Name == "Keywords" && p.IsActive)
                      .SelectMany(p => JsonConvert.DeserializeObject<List<string>>(p.Value))
                      .Any(kw => textoMinuscula.Contains(kw.ToLower()));

                if (requestSupport
                    && convoDto.Status.ToString() == ConversationStatus.Bot.ToString())
                {
                    // Definir los botones que ya tenías:
                    var buttons1 = new[]
                    {
                            new WhatsAppInteractiveButton { Id = "1", Title = "Seguir con chatbot" },
                            new WhatsAppInteractiveButton { Id = "2", Title = "Contactar con soporte" }
                    };

                    // Enviar el mensaje interactivo con lista de opciones
                    await _whatsAppService.SendInteractiveButtonsAsync(
                        convoDto.ConversationId,
                        BotUserId,
                        "Parece que deseas hablar con un agente. ¿Qué prefieres?",
                        buttons1,
                        ct
                    );

                    var updatedConv = await _conversationService.GetByIdAsync(convoDto.ConversationId, ct);

                    await _hubContext.Clients
                               .Group("Admin")
                               .SendAsync("ConversationUpdated", updatedConv, ct);

                    // Terminar aquí el procesamiento (no pasar a flujo de bot/texto normal)
                    return;
                }
                else if (requestSupport
                    && convoDto.Status.ToString() != ConversationStatus.Bot.ToString())
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
                        .SendAsync("ConversationUpdated", convoDto, ct);
                    return;
                }
            }


            // 2) Si traemos media… (imagen, video, audio, sticker, documento)
            if (msg.Image != null
             || msg.Video != null
             || msg.Audio != null
             || msg.Sticker != null
             || msg.Document != null)
            {
                // 2.1) Extraer mediaId, mimeType y caption
                string mediaId = msg.Image?.Id
                               ?? msg.Video?.Id
                               ?? msg.Audio?.Id
                               ?? msg.Sticker?.Id
                               ?? msg.Document!.Id!;
                string mimeType = msg.Image?.MimeType
                                ?? msg.Video?.MimeType
                                ?? msg.Audio?.MimeType
                                ?? msg.Sticker?.MimeType
                                ?? msg.Document!.MimeType;
                string? caption = msg.Caption;

                // 2.2) Descargar URL y binario
                var downloadUrl = await _whatsAppService.DownloadMediaUrlAsync(mediaId, ct);
                var data = await _whatsAppService.DownloadMediaAsync(downloadUrl, ct);

                // 2.3) Guardar el fichero en wwwroot/media y montar URL pública
                var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media");
                Directory.CreateDirectory(wwwroot);

                // inferir extensión a partir del mimeType o del filename original
                var ext = mimeType switch
                {
                    "image/jpeg" => ".jpeg",
                    "image/jpg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    "image/gif" => ".gif",
                    "image/tiff" => ".tiff",
                    "image/heif" => ".heif",
                    "image/raw" => ".raw",
                    "image/bmp" => ".bmp",
                    "image/ico" => ".ico",
                    "video/mp4" => ".mp4",
                    "audio/ogg" or "audio/opus" => ".ogg",
                    "application/pdf" => ".pdf",
                    "application/msword" => ".doc",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                    "application/vnd.ms-excel" => ".xls",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
                    "application/vnd.openxmlformats-officedocument.presentationml.presentation" => ".pptx",
                    // si viene un documento genérico con nombre original, usar su extensión
                    _ when msg.Document?.Filename is not null => Path.GetExtension(msg.Document.Filename)!,
                    _ => ".bin"
                };

                var fileName = $"{mediaId}{ext}";
                var filePath = Path.Combine(wwwroot, fileName);
                await File.WriteAllBytesAsync(filePath, data, ct);

                // construir tu URL pública contra la API
                var req = _httpContextAccessor.HttpContext!.Request;
                var baseUrl = $"{req.Scheme}://{req.Host}";
                var publicUrl = $"{baseUrl}/media/{fileName}";

                // 2.4) Persistir el mensaje en BD (como contacto)
                var sendReq = new SendMessageRequest
                {
                    ConversationId = convoDto.ConversationId,
                    SenderId = contactDto.Id,
                    Content = caption,
                    MessageType = mimeType switch
                    {
                        var m when m.Equals("image/webp", StringComparison.OrdinalIgnoreCase) => MessageType.Sticker,
                        var m when m.StartsWith("image/") => MessageType.Image,
                        var m when m.StartsWith("video/") => MessageType.Video,
                        var m when m.StartsWith("audio/") => MessageType.Audio,
                        _ => MessageType.Document
                    }
                };
                var messageDto = await _messageService.SendMessageAsync(sendReq, isContact: true, CancellationToken.None);

                // 2.5) Guardar el attachment en BD con TU URL pública
                var attachReq = new UploadAttachmentRequest
                {
                    MessageId = messageDto.MessageId,
                    MediaId = mediaId,
                    MimeType = mimeType,
                    FileName = fileName,
                    MediaUrl = publicUrl
                };
                await _attachmentService.UploadAsync(attachReq, CancellationToken.None);

                return;
            }

            var buttons = new[]
                {
                    new WhatsAppInteractiveButton { Id = "1", Title = "Seguir con chatbot" },
                    new WhatsAppInteractiveButton { Id = "2", Title = "Hablar con soporte" }
                };

            if (!convoDto.Initialized)
            {
                if (contactDto.Status == ContactStatus.Completed)
                {

                    var BotGreetings = string.Format(systemParam.FirstOrDefault(sp => sp.Name == "BotGreetings")?.Value ?? "¿Cual es su numero de cedula?", contactDto.FullName);

                    await _messageService.SendMessageAsync(
                            new SendMessageRequest
                            {
                                ConversationId = convDto.ConversationId,
                                SenderId = 1,
                                //Content = _prompts.DataComplete,
                                Content = BotGreetings,
                                MessageType = MessageType.Text
                            },
                            isContact: false,
                            ct);

                    // 3) emítelo por SignalR
                    await _hubContext.Clients
                       .All
                       .SendAsync("ConversationUpdated", ct, ct);
                }

                _uow.ClearChangeTracker();

                await _conversationService.UpdateAsync(new UpdateConversationRequest
                {
                    ConversationId = convoDto.ConversationId,
                    Initialized = true,
                    Priority = PriorityLevel.Normal,
                    Status = ConversationStatus.Bot,
                    IsArchived = false
                }, ct);

                //await _messageService.SendMessageAsync(new SendMessageRequest
                //{
                //    ConversationId = convoDto.ConversationId,
                //    SenderId = BotUserId,
                //    Content = "Hola, soy *Milena*, tu asistente virtual de PC GROUP S.A. ¿En qué puedo ayudarte?",
                //    MessageType = MessageType.Text
                //}, false, ct);

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
                        if (convDto.Status != ConversationStatus.Human.ToString())
                        {
                            _uow.ClearChangeTracker();

                            await _conversationService.UpdateAsync(new UpdateConversationRequest
                            {
                                ConversationId = convoDto.ConversationId,
                                Status = ConversationStatus.Bot
                            }, ct);

                            //await _messageService.SendMessageAsync(new SendMessageRequest
                            //{
                            //    ConversationId = convoDto.ConversationId,
                            //    SenderId = BotUserId,
                            //    Content = "Perfecto, continuemos. ¿En qué más puedo ayudarte?",
                            //    MessageType = MessageType.Text
                            //}, false, ct);
                            await _messageService.SendMessageAsync(new SendMessageRequest
                            {
                                ConversationId = convoDto.ConversationId,
                                SenderId = BotUserId,
                                //Content = _prompts.WelcomeBot,
                                Content = systemParam.FirstOrDefault(p => p.Name == "WelcomeBot")?.Value ?? "Perfecto, continuemos. ¿En qué más puedo ayudarte?",
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
                        if (convDto.Status == ConversationStatus.Bot.ToString())
                        {
                            var nowNic = await _nicDatetime.GetNicDatetime();
                            _uow.ClearChangeTracker();

                            var isHoliday = await _openingHourService.IsHolidayAsync(DateOnly.FromDateTime(DateTime.Now));
                            var isOutOfOpeningHour = await _openingHourService.IsOutOfOpeningHourAsync(nowNic, ct);

                            if (isHoliday || isOutOfOpeningHour)
                            {
                                var workShift = await _workShiftService.GetMembersOnShiftAsync(DateTime.Now, ct);

                                if(workShift != null)
                                {
                                    var data = workShift.First().UserId;
                                }

                                //El first  es temporal, en posterios actualizaciones se puede aumentar la cantidad.
                                await _conversationService.AutoAssingAsync(convDto.ConversationId, workShift.First().UserId, ct);

                                return;
                            }

                            await _conversationService.UpdateAsync(new UpdateConversationRequest
                            {
                                ConversationId = convoDto.ConversationId,
                                Status = ConversationStatus.Waiting,
                                RequestedAgentAt = nowNic,

                            }, ct);

                            await _messageService.SendMessageAsync(new SendMessageRequest
                            {
                                ConversationId = convoDto.ConversationId,
                                SenderId = BotUserId,
                                //Content = _prompts.SupportRequestReceived,
                                Content = systemParam.FirstOrDefault(p => p.Name == "SupportRequestReceived")?.Value ?? "Solicitud de soporte recibida. Un agente se pondrá en contacto contigo pronto.",
                                MessageType = MessageType.Text
                            }, false, ct);

                            var updatedConv = await _conversationService.GetByIdAsync(convoDto.ConversationId, ct);

                            var usersAdmin = await _userService.GetByRoleAsync("Admin", ct);

                            var agents = usersAdmin
                                .Where(u => u.IsActive)
                                .Select(u => u.UserId)
                                .ToArray();

                            //await _notification.CreateAsync(
                            //   NotificationType.SupportRequested,
                            //   $"EL cliente {contactDto.WaName} ha solicitado atención por un agente de soporte.",
                            //   agents,
                            //   ct);

                            var payloadHub = new
                            {
                                ConversationId = convDto.ConversationId,
                                ClientName = contactDto.FullName != null ? contactDto.FullName : contactDto.WaName,
                                RequestedAt = nowNic
                            };

                            await _hubNotification
                                 .Clients
                                 .Group("Admins")             // coincide con tu OnConnectedAsync
                                 .SendAsync("SupportRequested", payloadHub, ct);

                            ///Validar que solo llegue a los admins
                            await _hubContext.Clients
                                .Group("Admin")
                                .SendAsync("ConversationUpdated", updatedConv, ct);
                        }
                        else
                        {
                            

                            await _messageService.SendMessageAsync(new SendMessageRequest
                            {
                                ConversationId = convoDto.ConversationId,
                                SenderId = BotUserId,
                                Content = systemParam.FirstOrDefault(p => p.Name == "WaitingQueue")?.Value ?? "ℹ️ Lo sentimo su conversación esta siendo atendida por un agente de soporte, por el momento deve de finalizar la conversación actual.",
                                MessageType = MessageType.Text
                            }, false, ct);

                            await _hubContext.Clients
                               .All
                               .SendAsync("ConversationUpdated", convDto, ct);
                        }

                        return;
                }
            }

            if (convDto.Status == ConversationStatus.Waiting.ToString())
            {
                await _messageService.SendMessageAsync(new SendMessageRequest
                {
                    ConversationId = convoDto.ConversationId,
                    SenderId = BotUserId,
                    Content = systemParam.FirstOrDefault(p => p.Name == "RequestUnderReviewMessage")?.Value ?? "⏳ Su solicitud está en proceso. Un agente le atenderá en breve, por favor espere un momento.",
                    MessageType = MessageType.Text
                }, false, ct);

                await _hubContext.Clients
                   .All
                   .SendAsync("ConversationUpdated", convDto, ct);
                return;
            }

            if (convoDto.Status == ConversationStatus.Bot.ToString())
            {
                await HandleBotReplyAsync(convoDto, payload.TextBody, ct);
                return;
            }
        }

        private async Task HandleBotReplyAsync(
            ConversationResponseDto convoDto,
            string? userText,
            CancellationToken ct = default)
        {
            try
            {
                var allTexts = (await _messageService
                    .GetByConversationAsync(convoDto.ConversationId, ct))
                    .Select(m => m.Content)
                    .Where(t => !string.IsNullOrWhiteSpace(t));

                var jsonPath = Path.Combine(_env.ContentRootPath, "WhContext", "websiteContext.json");
                WebsiteContextDto websiteCtx;
                if (File.Exists(jsonPath))
                {
                    var websiteJson = await File.ReadAllTextAsync(jsonPath, ct);
                    websiteCtx = System.Text.Json.JsonSerializer.Deserialize<WebsiteContextDto>(websiteJson)!;
                }
                else
                {
                    websiteCtx = new WebsiteContextDto { Content = "", UpdatedAtUtc = DateTime.MinValue };
                }

                var fullPrompt = $@"
                        Resumen breve sobre quien eres:
                        {_systemPrompt}

                        Información del sitio web (actualizado {websiteCtx.UpdatedAtUtc:u}):
                        {websiteCtx.Content}

                        Historial de mensajes:
                        {string.Join('\n', allTexts)}

                        Responde a: {userText}";

                var botReply = (await _geminiClient
                    .GenerateContentAsync(fullPrompt, userText ?? "", ct))
                    .Trim();

                var sendReq = new SendMessageRequest
                {
                    ConversationId = convoDto.ConversationId,
                    SenderId = BotUserId,
                    Content = botReply
                };

                await _messageService.SendMessageAsync(sendReq, false);

                var convEntity = await _uow.Conversations.GetAll()
                    .Where(c => c.ConversationId == convoDto.ConversationId)
                    .Include(c => c.ClientContact)
                    .SingleAsync();

                var convDto = convEntity.Adapt<ConversationResponseDto>();

                await _hubContext.Clients
                    .All
                    .SendAsync("ConversationUpdated", convDto, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}