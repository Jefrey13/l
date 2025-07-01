using System;
using Microsoft.AspNetCore.Http;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class SendMessageRequest
    {
        public string? MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string? Content { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Text;
        public IFormFile? File { get; set; }
        public string? Caption { get; set; }

        public string? InteractiveId { get; set; }
        public string? InteractiveTitle { get; set; }

    }
}
