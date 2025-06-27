using System;
using CustomerService.API.Models;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class ContactLogResponseDto
    {
        public int Id { get; set; }
        public string WaName { get; set; } = "";
        public string WaId { get; set; } = "";
        public string WaUserId { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? IdCard { get; set; }
        public string? ResidenceCard { get; set; }
        public string? Password { get; set; }
        public IdType IdType { get; set; } //Tipo de documento de idetificación.
        public string? FullName { get; set; }

        public string? CompanyName { get; set; }

        public int? CompanyId { get; set; }
        public bool? IsProvidingData { get; set; }
        public bool IsVerified { get; set; }
        public ContactStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Company? Company { get; set; }

        public DateTime? VerifiedAt { get; set; }
        public int? verifiedId { get; set; }
        //public virtual User? VerifiedBy { get; set; } = new User();
    }
}
