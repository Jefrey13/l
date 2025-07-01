using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class StatusHistoryResponseDto
    {
        public MessageStatus Status { get; set; }
        public long Timestamp { get; set; }
        public string? Metadata { get; set; }
    }
}