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
using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IHubContext<ChatHub> _hub;
        private readonly INicDatetime _nicDatetime; 

        public MessageService(
            IUnitOfWork uow,
            IWhatsAppService whatsAppService,
            IHubContext<ChatHub> hubContext,
            INicDatetime nicDatetime)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _whatsAppService = whatsAppService ?? throw new ArgumentNullException(nameof(whatsAppService));
            _hub = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _nicDatetime = nicDatetime;
        }

        public async Task<MessageDto> SendMessageAsync(
            SendMessageRequest request,
            bool isContact = false,
            CancellationToken ct = default)
        {
            if (request.ConversationId <= 0)
                throw new ArgumentException("ConversationId must be greater than zero.", nameof(request.ConversationId));

            var msg = new Message
            {
                ConversationId = request.ConversationId,
                SenderUserId = isContact ? null : request.SenderId,
                SenderContactId = isContact ? request.SenderId : null,
                Content = request.Content,
                MessageType = request.MessageType,
                SentAt = await _nicDatetime.GetNicDatetime(),
                Status = MessageStatus.Sent,
                ExternalId = Guid.NewGuid().ToString(),
                InteractiveId = request.InteractiveId,
                InteractiveTitle = request.InteractiveTitle
            };

            await _uow.Messages.AddAsync(msg, ct);
            await _uow.SaveChangesAsync(ct);

            if (!isContact)
            {
                await _whatsAppService.SendTextAsync(
                    msg.ConversationId,
                    msg.SenderUserId!.Value,
                    msg.Content,
                    ct);
            }

            var reloaded = await _uow.Messages.GetAll()
                .Where(m => m.MessageId == msg.MessageId)
                .Include(m => m.SenderUser)
                .Include(m => m.SenderContact)
                .Include(m => m.Attachments)
                .SingleAsync(ct);

            var dto = reloaded.Adapt<MessageDto>();

            await _hub.Clients
               .Group(reloaded.ConversationId.ToString())
               .SendAsync("ReceiveMessage", dto, ct);

            return dto;
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