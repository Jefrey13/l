namespace CustomerService.API.Dtos.ResponseDtos
{
    public class ConversationStatusCountResponseDto
    {
        public string Status { get; set; } = default!;
        public int Count { get; set; }
    }
}