using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class ConversationHistoryDto
    {
        public int ConversationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ConversationStatus Status { get; set; }
        public IEnumerable<MessageWithAttachmentsResponseDto> Messages { get; set; }
    }
}
