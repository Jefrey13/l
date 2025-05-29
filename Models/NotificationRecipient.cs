using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerService.API.Models
{
    public class NotificationRecipient
    {
        public int NotificationRecipientId { get; set; }

        public int NotificationId { get; set; }
        [ForeignKey(nameof(NotificationId))]
        public Notification Notification { get; set; } = null!;

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
    }
}
