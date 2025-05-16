using CustomerService.API.Models;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class ContactLogResponseDto
    {
        public int Id { get; set; }
        public string WaName { get; set; } // String. The customer's name.
        public string WaId { get; set; } // String. The customer's WhatsApp ID.
        public string WaUserId { get; set; } // String.Additional unique, alphanumeric identifier for a WhatsApp user.
        public string Phone { get; set; }
        public string? IdCard { get; set; }
        public string? FullName { get; set; }
        public int? CompanyId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public Company? Company { get; set; }
    }
}