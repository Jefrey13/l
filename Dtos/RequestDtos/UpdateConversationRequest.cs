using CustomerService.API.Utils.Enums;
using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class UpdateConversationRequest
    {
        [Required]
        public int ConversationId { get; set; }
        public PriorityLevel? Priority { get; set; }
        public bool? Initialized { get; set; }
        public ConversationStatus? Status { get; set; }
        public int? AssignedAgentId { get; set; }

        public DateTime? RequestedAgentAt { get; set; }
        public List<string>? Tags { get; set; }
        public bool? IsArchived { get; set; }
    }
}
