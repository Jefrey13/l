using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Utils.Enums;

public class ConversationDto
{
    public int ConversationId { get; set; }
    public int? CompanyId { get; set; }
    public int ClientContactId { get; set; }
    public string? ClientContactName { get; set; }
    public string? ContactNumber { get; set; }
    public PriorityLevel Priority { get; set; }
    public int? AssignedAgentId { get; set; }

    public string? AssignedAgentName { get; set; }
    public int? AssignedByUserId { get; set; }

    public string? AssignedByUserName { get; set; }
    public DateTime? AssignedAt { get; set; }
    public string Status { get; set; }
    public bool Initialized { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool IsArchived { get; set; }
    public int TotalMessages { get; set; }
    public DateTime LastActivity { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan? TimeToFirstResponse { get; set; }
    public bool IsClosed { get; set; }
    public List<MessageDto> Messages { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();
}