using CustomerService.API.Models;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class UpdateContactLogRequestDto
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public IdType IdType { get; set; } //Tipo de documento de idetificación.
        public string? IdCard { get; set; }
        public string? ResidenceCard { get; set; }
        public string? Passport { get; set; }
        public string? FullName { get; set; }
        public string? CompanyName { get; set; }
        public bool? IsProvidingData { get; set; }
        public int CompanyId { get; set; }
        public ContactStatus Status { get; set; }
    }
}