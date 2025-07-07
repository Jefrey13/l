using CustomerService.API.Models;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public string Payload { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public NotificationType Type { get; set; }

        public virtual ICollection<NotificationRecipientResponseDto> Recipients { get; set; }

            = new List<NotificationRecipientResponseDto>();
    }
}