namespace CustomerService.API.Dtos.RequestDtos.Wh
{
    public class StatusEntry
    {
        public List<StatusChange> Changes { get; set; } = new();
    }
}
