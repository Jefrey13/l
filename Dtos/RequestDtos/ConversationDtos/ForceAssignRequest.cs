using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos.ConversationDtos
{
    public class ForceAssignRequest
    {
        [Required]
        public int TargetAgentId { get; set; }
        [Required]
        public string Comment { get; set; }
    }
}
