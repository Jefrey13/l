using CustomerService.API.Utils.Enums;
using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class StartConversationRequest
    {
        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int ClientContactId { get; set; }

        public PriorityLevel Priority { get; set; } = PriorityLevel.Normal;

        public List<int>? TagIds { get; set; }
    }
}