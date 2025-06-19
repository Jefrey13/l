namespace CustomerService.API.Models
{
    public class MessageRead
    {
        public int MessageReadId { get; set; }

        public int MessageId { get; set; }
        public Message Message { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    }
}
