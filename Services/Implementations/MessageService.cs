using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Hubs;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils.Enums;
using Mapster;
using Microsoft.AspNetCore.SignalR;

namespace CustomerService.API.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IHubContext<ChatHub> _hub;

        public MessageService(
            IUnitOfWork uow,
            IWhatsAppService whatsAppService,
            IHubContext<ChatHub> hubContext)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _whatsAppService = whatsAppService ?? throw new ArgumentNullException(nameof(whatsAppService));
            _hub = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task SendMessageAsync(SendMessageRequest request, CancellationToken cancellation = default)
        {
            if (request.ConversationId <= 0)
                throw new ArgumentException("ConversationId must be greater than zero.", nameof(request.ConversationId));

            var msg = new Message
            {
                ConversationId = request.ConversationId,
                SenderUserId = request.SenderId,
                Content = request.Content,
                MessageType = request.MessageType,
                SentAt = DateTimeOffset.UtcNow,
                Status = MessageStatus.Sent,
                ExternalId = Guid.NewGuid().ToString()
            };

            //if (request.File != null)
            //{
            //    var attachment = new Attachment
            //    {
            //        FileName = request.File.FileName,
            //        MimeType = request.File.ContentType,
            //        MediaUrl = await _whatsAppService.UploadMediaAsync(request.File, cancellation)
            //    };
            //    msg.Attachments.Add(attachment);
            //}

            await _uow.Messages.AddAsync(msg, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            // Enviar via WhatsApp Cloud API
            //if (msg.Attachments.Any())
            //{
            //    await _whatsAppService.SendMediaAsync(
            //        msg.ConversationId,
            //        msg.SenderUserId ?? 0,
            //        msg.ExternalId,
            //        cancellation);
            //}
            //else
            //{
            await _whatsAppService.SendTextAsync(
                msg.ConversationId,
                msg.SenderUserId ?? 0,
                msg.Content,
                cancellation);
            //}

            var dto = msg.Adapt<MessageDto>();
            await _hub.Clients
                .Group(msg.ConversationId.ToString())
                .SendAsync("ReceiveMessage", dto, cancellation);
        }

        public async Task<IEnumerable<MessageDto>> GetByConversationAsync(int conversationId, CancellationToken cancellation = default)
        {
            if (conversationId <= 0)
                throw new ArgumentException("ConversationId must be greater than zero.", nameof(conversationId));

            var messages = await _uow.Messages.GetByConversationAsync(conversationId, cancellation);
            return messages.Adapt<IEnumerable<MessageDto>>();
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
    }
}