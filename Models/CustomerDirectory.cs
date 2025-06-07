namespace CustomerService.API.Models
{
    public class CustomerDirectory
    {
        public int Id { get; set; }
        public string CustomerCode { get; set; }
        public string LegalName { get; set; }
        public string TradeName { get; set; }
        public string Department { get; set; }
        public string BusinessActivity { get; set; }
        public string IdentificationNumber { get; set; }
        public string PhoneNumbers { get; set; }
        public string MobilePhone { get; set; }
        public string AlternateMobilePhone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Service { get; set; }
        public string AgentName { get; set; }
        public int NumberOfContacts { get; set; }
        public int NumberOfLocations { get; set; }
        public string ContactLegalRepresentative { get; set; }
        public string LegalRepresentativeName { get; set; }
        public string LegalRepresentativeID { get; set; }
        public string LegalRepresentativePhones { get; set; }
        public string BusinessHours { get; set; }
        public string CallHours { get; set; }
        public string RegistrationDate { get; set; }
        public string WorkType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; } = null!;
        public bool? IsActive { get; set; }

        public virtual ICollection<ContactLog> ContactLogs { get; set; }
            = new List<ContactLog>();
    }
}
