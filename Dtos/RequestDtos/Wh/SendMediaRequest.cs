namespace CustomerService.API.Dtos.RequestDtos.Wh
{
    public class SendMediaRequest
    {
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public byte[] Data { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public string? Caption { get; set; }
    }
}
