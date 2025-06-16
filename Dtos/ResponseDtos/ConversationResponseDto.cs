using CustomerService.API.Utils.Enums;
using System;
using System.Collections.Generic;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class ConversationResponseDto
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
        public string? Justification { get; set; }
        public string Status { get; set; } = null!;
        public bool Initialized { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AssignedAt { get; set; }
        public DateTime? FirstResponseAt { get; set; }
        public DateTime? ClientLastMessageAt { get; set; }
        public DateTime? AgentFirstMessageAt { get; set; }
        public DateTime? AgentLastMessageAt { get; set; }
        public DateTime? RequestedAgentAt { get; set; }
        public DateTime? AgentRequestAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool IsArchived { get; set; }
        public int TotalMessages { get; set; }
        public DateTime LastActivity { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan? TimeToFirstResponse { get; set; }

        // Fecha en que el support respondió (aceptó/rechazó)
        public DateTime? AssignmentResponseAt { get; set; }

        // Motivo de rechazo o de fuerza
        public string? AssignmentComment { get; set; }
        public bool IsClosed { get; set; }

        public List<MessageResponseDto> Messages { get; set; } = new();

        public List<string> Tags { get; set; } = new();
    }
}
