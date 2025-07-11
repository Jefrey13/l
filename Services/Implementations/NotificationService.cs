﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;
using Mapster;

namespace CustomerService.API.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notifications;
        private readonly INotificationRecipientRepository _recipients;
        private readonly IUnitOfWork _uow;
        private readonly INicDatetime _nicDatetime;
        private readonly ISignalRNotifyService _signalR;

        public NotificationService(
            INotificationRepository notifications,
            INotificationRecipientRepository recipients,
            IUnitOfWork uow,
            ISignalRNotifyService signalR,
            INicDatetime nicDatetime)
        {
            _notifications = notifications;
            _recipients = recipients;
            _uow = uow;
            _signalR = signalR;
            _nicDatetime = nicDatetime;
        }

        public async Task CreateAsync(NotificationType type, string payload, IEnumerable<int> recipientUserIds, CancellationToken ct = default)
        {
            try
            {
                var localDate = await _nicDatetime.GetNicDatetime();

                var notification = new Notification
                {
                    Type = type,
                    Payload = payload,
                    CreatedAt = localDate,
                };

                //var newNotification = await _notifications.AddNotificationAsync(notification, ct);
                await _uow.Notifications.AddAsync(notification, ct);
                await _uow.SaveChangesAsync(ct);

                foreach (var userId in recipientUserIds)
                {
                    var rec = new NotificationRecipient
                    {
                        NotificationId = notification.Id,
                        UserId = userId
                    };

                    await _recipients.AddAsync(rec, ct);
                }

                await _uow.SaveChangesAsync(ct);

                //await _signalR.SendNotificationToUsersAsync(recipientUserIds, notification.Adapt<NotificationResponseDto>());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}