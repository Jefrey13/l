namespace CustomerService.API.Dtos.RequestDtos
{
        public class SupportRequestedDto
        {
            public int ConversationId { get; set; }
            public string Phone { get; set; } = null!;
            public string? WaName { get; set; }
        }
}
