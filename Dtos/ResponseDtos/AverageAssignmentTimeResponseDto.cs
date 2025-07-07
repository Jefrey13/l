namespace CustomerService.API.Dtos.ResponseDtos
{
    public class AverageAssignmentTimeResponseDto
    {
        public int AgentId { get; set; }
        public string AgentName { get; set; }
        public double AverageSeconds { get; set; }
    }
}
