namespace CustomerService.API.Dtos.RequestDtos
{
        public class ConversationAssignedDto
        {
            public int ConversationId { get; set; }
            public int AssignedAgentId { get; set; }
            public int AssignedByUserId { get; set; }
        }
}
