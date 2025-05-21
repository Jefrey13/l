using System;
using System.Collections.Generic;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int? SenderUserId { get; set; }
        public int? SenderContactId { get; set; }
        public bool IsIncoming { get; set; }
        public string? Content { get; set; }
        public string ExternalId { get; set; } = string.Empty;
        public MessageType MessageType { get; set; }

        public string? InteractiveId { get; set; }
        public string? InteractiveTitle { get; set; }
        public string Status { get; set; }
        public DateTimeOffset SentAt { get; set; }
        public DateTimeOffset? DeliveredAt { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new();
    }
}