using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.RequestDtos.ConversationDtos
{
        public class UpdateConversationRequestDto
        {    public int ConversationId { get; set; }
            public AssignmentState? Status { get; set; }
            public string Justification { get; set; } = string.Empty;
        }
}