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
using WhatsappBusiness.CloudApi.Messages.Requests;
using Microsoft.AspNetCore;
using CustomerService.API.Hubs;
using CustomerService.API.Utils;
using System.Text.Json.Serialization;
using WhatsappBusiness.CloudApi.Response;
using Message = CustomerService.API.Models.Message;

namespace CustomerService.API.Services.Implementations
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _http;
        private readonly IUnitOfWork _uow;
        //private readonly IMessageService _messageService;
        private readonly IContactLogService _contactService;
        private readonly ISignalRNotifyService _signalR;
        private readonly string _token;
        private readonly string _phoneNumberId;
        private readonly string _version;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly INicDatetime _nicDatetime;

        public WhatsAppService(
            HttpClient http,
            IUnitOfWork uow,
            //IMessageService messageService,
            IContactLogService contactService,
            ISignalRNotifyService signalR,
            IConfiguration config,
            IHubContext<ChatHub> hubContext,
            INicDatetime nicDatetime)
        {
            _http = http;
            _uow = uow;
            //_messageService = messageService;
            _contactService = contactService;
            _signalR = signalR;
            _token = config["WhatsApp:Token"]!;
            _phoneNumberId = config["WhatsApp:PhoneNumberId"]!;
            _version = config["WhatsApp:ApiVersion"]!;
            _hubContext = hubContext;
            _nicDatetime = nicDatetime;
        }

        public async Task SendTextAsync(int conversationId, int senderId, string text, CancellationToken cancellation = default)
        {
            try
            {

            var convo = await _uow.Conversations.GetAll()
                .Where(c => c.ConversationId == conversationId)
                .Select(c => new { c.ConversationId, Phone = c.ClientContact.Phone })
                .SingleOrDefaultAsync()
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
            var res = await _http.SendAsync(req);

            res.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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

            var listSections = new[]{
            new {
                title = "Opciones disponibles",
                rows = buttons.Select(b => new {
                    id = b.Id,
                    title = b.Title,
                    description = b.Description
                }).ToArray()
            }
        };

            var interactive = new
            {
                type = "list",
                header = new { type = "text", text = header },
                body = new { text = "Por favor indique por el medio que desea seguir comunicandose:" },
                footer = new { text = "Click y seleccione una opcion." },
                action = new
                {
                    button = "Seleccionar",
                    sections = listSections
                }
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

            var incoming = new Message
            {
                ConversationId = conversationId,
                //El que envie mensaje es el bot, por lo cual se asocia con los usuario y no con contactos.
                SenderUserId = senderId,
                Content =
                    header + "\n" +
                    interactive.body.text + "\n" +
                    interactive.footer.text + "\n\n" +
                    "Opciones:\n" +
                    string.Join(
                        "\n",
                        buttons.Select(b => $"- {b.Title}")
                    ),
                SentAt = await _nicDatetime.GetNicDatetime(),
                Status = MessageStatus.Delivered
            };

            await _uow.Messages.AddAsync(incoming, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            var dto = incoming.Adapt<MessageResponseDto>();
            await _hubContext
                .Clients
                .Group(conversationId.ToString())
                .SendAsync("ReceiveMessage", dto, CancellationToken.None);

            //var dto = incoming.Adapt<MessageDto>();

            //await _signalR.NotifyUserAsync(
            //    conversationId,
            //    "ReceiveMessage",
            //    dto);
        }


        public async Task<string> UploadMediaAsync(
    byte[] data,
    string mimeType,
    string fileName = "file",
    CancellationToken cancellation = default)
        {
            var url = $"https://graph.facebook.com/{_version}/{_phoneNumberId}/media";

            using var content = new MultipartFormDataContent
    {
        { new StringContent("whatsapp"), "messaging_product" },

        {
            new ByteArrayContent(data) {
                Headers = { ContentType = new MediaTypeHeaderValue(mimeType) }
            },
            "file",
            fileName
        }
    };

            using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var res = await _http.SendAsync(req, cancellation);
            res.EnsureSuccessStatusCode();

            var body = await res.Content
                .ReadFromJsonAsync<MediaUploadResponse>(cancellation)
                ?? throw new InvalidOperationException("Respuesta vacía al subir media");

            return body.Id;
        }

        public async Task SendMediaAsync(
        int conversationId,
        int senderId,
        string mediaId,
        string mimeType,
        string? caption = null,
        CancellationToken cancellation = default)
        {
            var toPhone = await _uow.Conversations.GetAll()
                .Where(c => c.ConversationId == conversationId)
                .Select(c => c.ClientContact.Phone)
                .SingleAsync(cancellation);

            object payload = mimeType.StartsWith("image/")
                ? new { messaging_product = "whatsapp", to = toPhone, type = "image", image = new { id = mediaId, caption } }
            : mimeType.StartsWith("video/")
                ? new { messaging_product = "whatsapp", to = toPhone, type = "video", video = new { id = mediaId, caption } }
            : mimeType.StartsWith("audio/")
                ? new { messaging_product = "whatsapp", to = toPhone, type = "audio", audio = new { id = mediaId } }
            : mimeType.Equals("image/webp", StringComparison.OrdinalIgnoreCase)
                ? new { messaging_product = "whatsapp", to = toPhone, type = "sticker", sticker = new { id = mediaId } }
            : new
            { // documentos y otros
                messaging_product = "whatsapp",
                to = toPhone,
                type = "document",
                document = new { id = mediaId, filename = "file", caption }
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

        public async Task<string> DownloadMediaUrlAsync(
            string mediaId,
            CancellationToken cancellation = default)
        {
            try
            {
                var url = $"https://graph.facebook.com/{_version}/{mediaId}";
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var res = await _http.SendAsync(req, cancellation);
                res.EnsureSuccessStatusCode();

                var body = await res.Content.ReadFromJsonAsync<MediaUrlResponse>(cancellation);
                if (body?.Url is null)
                    throw new InvalidOperationException("No se obtuvo URL de media desde WhatsApp");
                return body.Url;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "";
            }
        }

        public async Task<byte[]> DownloadMediaAsync(string mediaUrl, CancellationToken cancellation = default)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, mediaUrl);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            using var res = await _http.SendAsync(req, cancellation);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsByteArrayAsync(cancellation);
        }
    }
}