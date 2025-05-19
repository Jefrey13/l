using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class UpdateConversationRequest
    {
        public int ConversationId { get; set; }
        public PriorityLevel? Priority { get; set; }
        public bool? Initialized { get; set; }
        public ConversationStatus? Status { get; set; }
        public int? AssignedAgentId { get; set; }
        public List<int>? TagIds { get; set; }
        public bool? IsArchived { get; set; }
    }
}
