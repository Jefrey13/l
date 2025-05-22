using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class CreateConversationRequest
    {
        public int CompanyId { get; set; }
        public int ClientContactId { get; set; }
        public PriorityLevel Priority { get; set; } = PriorityLevel.Normal;
        public List<string>? Tags { get; set; }
    }
}
