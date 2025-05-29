// Models/Notification.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Models
{
    [Index(nameof(Type))]
    [Index(nameof(CreatedAt))]
    public class Notification
    {
        public int NotificationId { get; set; }

        public NotificationType Type { get; set; }

        public string Payload { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<NotificationRecipient> Recipients { get; set; }

            = new List<NotificationRecipient>();
    }
}