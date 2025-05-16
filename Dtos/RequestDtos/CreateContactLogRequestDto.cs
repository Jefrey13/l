using CustomerService.API.Models;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class CreateContactLogRequestDto
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string? IdCard { get; set; }
        public string? FullName { get; set; }
        public int CompanyId { get; set; }
    }
}
