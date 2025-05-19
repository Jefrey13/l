using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils.Enums;
using Mapster;

namespace CustomerService.API.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notifications;
        private readonly INotificationRecipientRepository _recipients;
        private readonly IUnitOfWork _uow;
        private readonly ISignalRNotifyService _signalR;

        public NotificationService(
            INotificationRepository notifications,
            INotificationRecipientRepository recipients,
            IUnitOfWork uow,
            ISignalRNotifyService signalR)
        {
            _notifications = notifications;
            _recipients = recipients;
            _uow = uow;
            _signalR = signalR;
        }

        public async Task CreateAsync(NotificationType type, string payload, IEnumerable<int> recipientUserIds, CancellationToken cancellation = default)
        {
            var notification = new Notification
            {
                Type = type,
                Payload = payload,
                CreatedAt = DateTime.UtcNow
            };
            await _notifications.AddAsync(notification, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            foreach (var userId in recipientUserIds)
            {
                var rec = new NotificationRecipient
                {
                    NotificationId = notification.NotificationId,
                    UserId = userId
                };
                await _recipients.AddAsync(rec, cancellation);
            }
            await _uow.SaveChangesAsync(cancellation);

            await _signalR.SendNotificationToUsersAsync(recipientUserIds, notification.Adapt<NotificationDto>());
        }
    }
}