namespace CustomerService.API.Models
{
    public class ConversationTag
    {
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
