using CustomerService.API.Models;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class UpdateContactLogRequestDto
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string? IdCard { get; set; }
        public string? FullName { get; set; }
        public int CompanyId { get; set; }
        public ContactStatus Status { get; set; }
    }
}