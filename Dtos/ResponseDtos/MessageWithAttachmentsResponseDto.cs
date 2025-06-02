using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class MessageWithAttachmentsResponseDto
    {
        public int MessageId { get; set; }
        public int? SenderUserId { get; set; }
        public string? SenderUserName { get; set; }
        public int? SenderContactId { get; set; }
        public string? SenderContactName { get; set; }
        public string? Content { get; set; }
        public DateTimeOffset SentAt { get; set; }
        public MessageType MessageType { get; set; }
        public IEnumerable<AttachmentDto> Attachments { get; set; }
    }
}
