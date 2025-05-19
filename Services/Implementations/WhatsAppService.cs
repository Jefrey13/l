using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils.Enums;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CustomerService.API.Dtos.ResponseDtos;

namespace CustomerService.API.Services.Implementations
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _http;
        private readonly IUnitOfWork _uow;
        //private readonly IMessageService _messageService;
        private readonly IContactLogService _contactService;
        private readonly IConversationService _conversationService;
        private readonly ISignalRNotifyService _signalR;
        private readonly string _token;
        private readonly string _phoneNumberId;
        private readonly string _version;

        public WhatsAppService(
            HttpClient http,
            IUnitOfWork uow,
            //IMessageService messageService,
            IContactLogService contactService,
            IConversationService conversationService,
            ISignalRNotifyService signalR,
            IConfiguration config)
        {
            _http = http;
            _uow = uow;
            //_messageService = messageService;
            _contactService = contactService;
            _conversationService = conversationService;
            _signalR = signalR;
            _token = config["WhatsApp:Token"]!;
            _phoneNumberId = config["WhatsApp:PhoneNumberId"]!;
            _version = config["WhatsApp:ApiVersion"]!;
        }

        public async Task HandleWebhookAsync(WhatsAppWebhookRequestDto webhook, CancellationToken cancellation = default)
        {
            var phone = webhook.From;
            ContactLogResponseDto contact;
            try
            {
                contact = await _contactService.GetByPhoneAsync(phone, cancellation);
            }
            catch
            {
                var create = new CreateContactLogRequestDto
                {
                    Phone = phone,
                    WaName = webhook.Name,
                    WaId = webhook.WaId,
                    WaUserId = webhook.WaUserId
                };
                contact = await _contactService.CreateAsync(create, cancellation);
            }

            var conv = (await _conversationService.GetAllAsync(cancellation))
                .FirstOrDefault(c => c.ClientContactId == contact.Id && !c.IsClosed);
            if (conv == null)
            {
                var start = new StartConversationRequest
                {
                    CompanyId = contact.CompanyId ?? 0,
                    ClientContactId = contact.Id,
                    Priority = PriorityLevel.Normal
                };
                conv = await _conversationService.StartAsync(start, cancellation);
            }

            var incoming = new Message
            {
                ConversationId = conv.ConversationId,
                SenderContactId = contact.Id,
                Content = webhook.Text,
                MessageType = webhook.Type,
                SentAt = DateTimeOffset.UtcNow,
                Status = MessageStatus.Delivered
            };
            await _uow.Messages.AddAsync(incoming, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            var dto = incoming.Adapt<MessageDto>();
            await _signalR.NotifyUserAsync(
                conv.ConversationId,
                "ReceiveMessage",
                dto);

            if (webhook.Type == MessageType.Text && conv.Status == ConversationStatus.New)
            {
                var sr = new SupportRequestedDto
                {
                    ConversationId = conv.ConversationId,
                    Phone = phone,
                    WaName = webhook.Name
                };
                await _signalR.NotifySupportRequestedAsync(sr, cancellation);
            }
        }

        public async Task SendTextAsync(int conversationId, int senderId, string text, CancellationToken cancellation = default)
        {
            var convo = await _uow.Conversations.GetAll()
                .Where(c => c.ConversationId == conversationId)
                .Select(c => new { c.ConversationId, Phone = c.ClientContact.Phone })
                .SingleOrDefaultAsync(cancellation)
                ?? throw new KeyNotFoundException("Conversation not found");

            var url = $"https://graph.facebook.com/{_version}/{_phoneNumberId}/messages";
            var payload = new
            {
                messaging_product = "whatsapp",
                to = convo.Phone,
                type = "text",
                text = new { body = text }
            };
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var res = await _http.SendAsync(req, cancellation);
            res.EnsureSuccessStatusCode();
        }

        public async Task<string> UploadMediaAsync(byte[] data, string mimeType)
        {
            var url = $"https://graph.facebook.com/{_version}/{_phoneNumberId}/messages";
            using var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(data), "file", "upload" },
                { new StringContent("whatsapp"), "messaging_product" }
            };
            using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var body = await res.Content.ReadFromJsonAsync<MediaUploadResponse>();
            return body!.Id;
        }

        public async Task SendMediaAsync(int conversationId, int senderId, string mediaId, string mimeType, string? caption = null, CancellationToken cancellation = default)
        {
            var phone = await _uow.Conversations.GetAll()
                .Where(c => c.ConversationId == conversationId)
                .Select(c => c.ClientContact.Phone)
                .SingleAsync(cancellation);

            object payload = mimeType.StartsWith("image/")
                ? new { messaging_product = "whatsapp", to = phone, type = "image", image = new { id = mediaId, caption } }
                : mimeType.StartsWith("video/")
                ? new { messaging_product = "whatsapp", to = phone, type = "video", video = new { id = mediaId, caption } }
                : new { messaging_product = "whatsapp", to = phone, type = "document", document = new { id = mediaId, filename = "file", caption } };

            var url = $"https://graph.facebook.com/{_version}/{_phoneNumberId}/messages";
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var res = await _http.SendAsync(req, cancellation);
            res.EnsureSuccessStatusCode();
        }

        private class MediaUploadResponse
        {
            public string Id { get; set; } = "";
        }

        public async Task SendInteractiveButtonsAsync(
            int conversationId,
            int senderId,
            string header,
            IEnumerable<WhatsAppInteractiveButton> buttons,
            CancellationToken cancellation = default)
        {
            var toPhone = await _uow.Conversations.GetAll()
                    .Include(c => c.ClientContact)
                    .Where(c => c.ConversationId == conversationId)
                    .Select(c => c.ClientContact.Phone)
                    .SingleOrDefaultAsync(cancellation)
                        ?? throw new KeyNotFoundException("Conversation not found");

            // Construir la lista de botones según spec de WhatsApp
            var waButtons = buttons.Select(b => new
            {
                type = "reply_button",
                reply = new { id = b.Id, title = b.Title }
            });

            // Payload interactivo
            var interactive = new
            {
                type = "button",
                header = new { type = "text", text = header },
                body = new { text = "Selecciona una opción:" },
                action = new { buttons = waButtons }
            };

            var payload = new
            {
                messaging_product = "whatsapp",
                to = toPhone,
                type = "interactive",
                interactive
            };

            var url = $"https://graph.facebook.com/{_version}/{_phoneNumberId}/messages";
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var res = await _http.SendAsync(req, cancellation);
            res.EnsureSuccessStatusCode();
        }
    }
}