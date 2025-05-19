namespace CustomerService.API.Dtos.RequestDtos
{
    public class NewContactCreatedDto
    {
        public int ContactId { get; set; }
        public string Phone { get; set; } = null!;
        public string? WaName { get; set; }
    }
}
