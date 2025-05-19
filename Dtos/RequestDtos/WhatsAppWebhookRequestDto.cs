using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class WhatsAppWebhookRequestDto
    {
        public string WaId { get; set; } = null!;
        public string WaUserId { get; set; } = null!;
        public string From { get; set; } = null!;
        public string Name { get; set; } = null!; 
        public MessageType Type { get; set; }
        public string Text { get; set; } = null!;
        public string? MediaId { get; set; } 
        public string? Caption { get; set; } 
        public string MessageId { get; set; } = null!;
        public long Timestamp { get; set; } 
    }
}
