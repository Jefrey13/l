namespace CustomerService.API.Dtos.RequestDtos
{
    public class ChangeValue
    {
        public List<WARequestMessage>? Messages { get; set; } = new();
        public List<Contact>? Contacts { get; set; } = new();
        //public List<StatusDto>? Statuses { get; set; }
    }
}