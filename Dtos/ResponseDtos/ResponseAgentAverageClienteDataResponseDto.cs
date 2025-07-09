namespace CustomerService.API.Dtos.ResponseDtos
{
    public class ResponseAgentAverageClienteDataResponseDto
    {
        public int? ClienteId { get; set; }
        public string? ClientName { get; set; }
        public double? AverageSeconds { get; set; }
        public int ConversationCount { get; set; } = 0;
    }
}
