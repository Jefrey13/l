using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class NotificationRecipientResponseDto
    {
        public int NotificationRecipientId { get; set; }
        public int NotificationId { get; set; }
        public NotificationType Type { get; set; }
        public string Payload { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}