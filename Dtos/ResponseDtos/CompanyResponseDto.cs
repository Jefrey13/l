namespace CustomerService.API.Dtos.ResponseDtos
{
    public class CompanyResponseDto
    {
        public int CompanyId { get; init; }
        public string Name { get; init; } = "";
        public DateTime CreatedAt { get; init; }

        public string? Address { get; init; }
    }
}