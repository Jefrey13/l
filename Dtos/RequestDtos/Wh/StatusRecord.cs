namespace CustomerService.API.Dtos.RequestDtos.Wh
{
    public class StatusRecord
    {
        public string Id { get; set; } = null!;         // externalMessageId
        public string Status { get; set; } = null!;     // "delivered" or "read"
        public long Timestamp { get; set; }              // epoch seconds
    }
}
