using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.RequestDtos.Wh;
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

namespace CustomerService.API.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IAttachmentService _attachSvc;
        private readonly IHubContext<ChatHub> _hub;
        private readonly INicDatetime _nicDatetime;
        private readonly ITokenService _tokenService;
        private readonly IHttpContextAccessor _ctx;

        public MessageService(
            IUnitOfWork uow,
            IWhatsAppService whatsAppService,
            IHubContext<ChatHub> hubContext,
            INicDatetime nicDatetime,
            IAttachmentService attachSvc,
            ITokenService tokenService,
            IHttpContextAccessor ctx)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _whatsAppService = whatsAppService ?? throw new ArgumentNullException(nameof(whatsAppService));
            _hub = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _nicDatetime = nicDatetime;
            _attachSvc = attachSvc;
            _tokenService = tokenService;
            _ctx = ctx;
        }

        public async Task<MessageResponseDto> SendMessageAsync(
            SendMessageRequest request,
            bool isContact = false,
            CancellationToken ct = default)
        {
            try
            {
                if (request.ConversationId <= 0)
                    throw new ArgumentException("ConversationId must be greater than zero.", nameof(request.ConversationId));

                // Crear el objeto Message con la hora nicaragüense
                var msg = new Message
                {
                    ConversationId = request.ConversationId,
                    SenderUserId = isContact ? null : request.SenderId,
                    SenderContactId = isContact ? request.SenderId : null,
                    Content = request.Content,
                    MessageType = request.MessageType,
                    SentAt = await _nicDatetime.GetNicDatetime(),  // hora nica
                    Status = MessageStatus.Sent,
                    ExternalId = Guid.NewGuid().ToString(),
                    InteractiveId = request.InteractiveId,
                    InteractiveTitle = request.InteractiveTitle
                };

                await _uow.Messages.AddAsync(msg, ct);
                await _uow.SaveChangesAsync(ct);

                // Recuperar la conversación para actualizar sus marcas de tiempo
                var conv = await _uow.Conversations.GetByIdAsync(request.ConversationId, ct)
                           ?? throw new KeyNotFoundException($"Conversation {request.ConversationId} not found");

                // msg.SentAt ya está en hora nica
                var sentAtNic = msg.SentAt.DateTime;

                // 3) Si el remitente es el cliente y aún no hay FirstResponseAt, se marca aquí
                if (conv.FirstResponseAt == null && isContact)
                {
                    conv.FirstResponseAt = sentAtNic;
                }

                // Siempre actualizamos UpdatedAt con hora nica
                conv.UpdatedAt = sentAtNic;

                // Si el remitente es un agente (no es contacto), actualizar sus marcas
                if (!isContact)
                {
                    if (conv.AgentFirstMessageAt == null)
                        conv.AgentFirstMessageAt = sentAtNic;

                    conv.AgentLastMessageAt = sentAtNic;

                    // Enviar el texto por WhatsApp
                    await _whatsAppService.SendTextAsync(
                        msg.ConversationId,
                        msg.SenderUserId!.Value,
                        msg.Content,
                        ct);
                }
                else
                {
                    // Si el remitente es cliente, actualizar ClientLastMessageAt
                    conv.ClientLastMessageAt = sentAtNic;
                }

                _uow.Conversations.Update(conv);
                await _uow.SaveChangesAsync(ct);

                // Recargar el mensaje y emitir por SignalR
                var reloaded = await _uow.Messages.GetByIdAsync(msg.MessageId);
                var dto = reloaded.Adapt<MessageResponseDto>();

                await _hub.Clients
                   .Group(reloaded.ConversationId.ToString())
                   .SendAsync("ReceiveMessage", dto, ct);

                return dto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new MessageResponseDto();
            }
        }

        public async Task<IEnumerable<MessageResponseDto>> GetByConversationAsync(int conversationId, CancellationToken cancellation = default)
        {
            if (conversationId <= 0)
                throw new ArgumentException("ConversationId must be greater than zero.", nameof(conversationId));

            var messages = await _uow.Messages.GetByConversationAsync(conversationId, cancellation);

            return messages.Adapt<IEnumerable<MessageResponseDto>>();
        }

        public async Task UpdateDeliveryStatusAsync(int messageId, DateTimeOffset deliveredAt, CancellationToken cancellation = default)
        {
            var msg = await _uow.Messages.GetByIdAsync(messageId, cancellation)
                      ?? throw new KeyNotFoundException($"Message {messageId} not found.");

            msg.DeliveredAt = deliveredAt;
            msg.Status = MessageStatus.Delivered;
            _uow.Messages.Update(msg);
            await _uow.SaveChangesAsync(cancellation);

            await _hub.Clients
                .Group(msg.ConversationId.ToString())
                .SendAsync("MessageDelivered", new { msg.MessageId, deliveredAt }, cancellation);
        }

        public async Task MarkAsReadAsync(int messageId, DateTimeOffset readAt, CancellationToken cancellation = default)
        {
            var msg = await _uow.Messages.GetByIdAsync(messageId, cancellation)
                      ?? throw new KeyNotFoundException($"Message {messageId} not found.");

            msg.ReadAt = readAt;
            msg.Status = MessageStatus.Read;
            _uow.Messages.Update(msg);
            await _uow.SaveChangesAsync(cancellation);

            await _hub.Clients
                .Group(msg.ConversationId.ToString())
                .SendAsync("MessageRead", new { msg.MessageId, readAt }, cancellation);
        }

        public async Task<MessageResponseDto> SendMediaAsync(
     SendMediaRequest req,
     string jwtToken,
     CancellationToken ct = default
 )
        {

            try
            {
                // 0) Extraer senderId del token
                var principal = _tokenService.GetPrincipalFromToken(jwtToken);
                var senderId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // 1) Subir media a WhatsApp Cloud
                var mediaId = await _whatsAppService
                    .UploadMediaAsync(req.Data, req.MimeType, req.FileName, CancellationToken.None);

                // 2) Enviar media por la API de WhatsApp
                await _whatsAppService
                    .SendMediaAsync(req.ConversationId, senderId, mediaId, req.MimeType, req.Caption, ct);

                // 3) Persistir el mensaje en BD directamente
                var msg = new Message
                {
                    ConversationId = req.ConversationId,
                    SenderUserId = senderId,
                    Content = req.Caption,
                    MessageType = req.MimeType switch
                    {
                        var m when m.Equals("image/webp", StringComparison.OrdinalIgnoreCase) => MessageType.Sticker,
                        var m when m.StartsWith("image/") => MessageType.Image,
                        var m when m.StartsWith("video/") => MessageType.Video,
                        var m when m.StartsWith("audio/") => MessageType.Audio,
                        _ => MessageType.Document
                    },
                    SentAt = await _nicDatetime.GetNicDatetime(),
                    Status = MessageStatus.Sent,
                    ExternalId = Guid.NewGuid().ToString()
                };
                await _uow.Messages.AddAsync(msg, ct);
                await _uow.SaveChangesAsync(ct);

                // 5) Guardar el binario en wwwroot/media y montar URL pública
                var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media");
                Directory.CreateDirectory(wwwroot);

                var ext = req.MimeType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    "image/gif" => ".gif",
                    "image/ico" => ".ico",
                    "video/mp4" => ".mp4",
                    "audio/mp3" => ".mp3",
                    "audio/ogg" or "audio/opus" => ".ogg",
                    "application/pdf" => ".pdf",
                    "application/msword" => ".doc",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                    "application/vnd.ms-excel" => ".xls",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
                    "application/vnd.openxmlformats-officedocument.presentationml.presentation" => ".pptx",
                    _ when Path.HasExtension(req.FileName) => Path.GetExtension(req.FileName)!,
                    _ => ".bin"
                };

                var fileName = $"{mediaId}{ext}";
                var filePath = Path.Combine(wwwroot, fileName);
                await File.WriteAllBytesAsync(filePath, req.Data, ct);

                // 6) Construir URL pública
                var httpReq = _ctx.HttpContext!.Request;
                var baseUrl = $"{httpReq.Scheme}://{httpReq.Host}";
                var publicUrl = $"{baseUrl}/media/{fileName}";

                // 7) Persistir attachment en BD
                var attach = new Attachment
                {
                    MessageId = msg.MessageId,
                    MediaId = mediaId,
                    MimeType = req.MimeType,
                    FileName = fileName,
                    MediaUrl = publicUrl,
                    CreatedAt = await _nicDatetime.GetNicDatetime()
                };
                await _uow.Attachments.AddAsync(attach, CancellationToken.None);
                await _uow.SaveChangesAsync(CancellationToken.None);


                // 4) Crear DTO y notificar por SignalR
                var reloadedMsg = await _uow.Messages.GetByIdAsync(msg.MessageId, CancellationToken.None);

                var dto = reloadedMsg.Adapt<MessageResponseDto>();
                await _hub.Clients
                    .Group(req.ConversationId.ToString())
                    .SendAsync("ReceiveMessage", dto, ct);

                return dto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new MessageResponseDto();
            }
        }
    }
}