namespace CustomerService.API.Dtos.ResponseDtos
{
    public class ResponseAgentAverageResponseDto
    {
        public int Id { get; set; }
        public string? AgentName { get; set; }
        public List<ResponseAgentAverageClienteDataResponseDto>? ClienteData { get; set; }
    }
}
