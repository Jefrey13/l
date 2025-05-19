using CustomerService.API.Models;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class CreateContactLogRequestDto
    {
        public string Phone { get; set; } = null!;
        public string? WaName { get; set; }
        public string? WaId { get; set; }
        public string? WaUserId { get; set; }
        public string? IdCard { get; set; }
        public string? FullName { get; set; }
        public int? CompanyId { get; set; }
    }
}
