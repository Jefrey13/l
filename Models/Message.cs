using CustomerService.API.Utils.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerService.API.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }

        public int? SenderUserId { get; set; }
        public int? SenderContactId { get; set; }

        public string? Content { get; set; }
        public string ExternalId { get; set; } = string.Empty;

        public string? InteractiveId { get; set; }
        public string? InteractiveTitle { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Text;

        public MessageStatus Status { get; set; } = MessageStatus.Sent;

        public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DeliveredAt { get; set; }
        public DateTimeOffset? ReadAt { get; set; }

        [ForeignKey(nameof(ConversationId))]
        public virtual Conversation Conversation { get; set; } = null!;

        [ForeignKey(nameof(SenderUserId))]
        public virtual User? SenderUser { get; set; }

        [ForeignKey(nameof(SenderContactId))]
        public virtual ContactLog? SenderContact { get; set; }
        public virtual ICollection<Attachment>? Attachments { get; set; } = new List<Attachment>();
    }
}