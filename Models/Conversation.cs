using CustomerService.API.Utils.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Models
{
    [Index(nameof(Status))]
    [Index(nameof(AssignedAgentId))]
    [Index(nameof(CreatedAt))]
    public class Conversation
    {
        public int ConversationId { get; set; }

        public int ClientContactId { get; set; }
        public PriorityLevel? Priority { get; set; } = PriorityLevel.Normal;

        public int? AssignedAgentId { get; set; }
        public int? AssignedByUserId { get; set; }
        public ConversationStatus? Status { get; set; } = ConversationStatus.New;
        public bool Initialized { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AssignedAt { get; set; }
        public DateTime? FirstResponseAt { get; set; }
        public DateTime? ClientFirstMessage { get; set; }
        public DateTime? ClientLastMessageAt { get; set; }
        public DateTime? AgentRequestAt { get; set; }
        public DateTime? AgentFirstMessageAt { get; set; }
        public DateTime? AgentLastMessageAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? IncompletedAt { get; set; }
        public string? Justification { get; set; } = null;

        public DateTime? WarningSentAt { get; set; }

        // Nuevo enum para saber en qué fase está la asignación
        public AssignmentState AssignmentState { get; set; } = AssignmentState.None;

        // Fecha en que el admin solicitó la asignación

        public DateTime? RequestedAgentAt { get; set; }

        // Fecha en que el support respondió (aceptó/rechazó)
        public DateTime? AssignmentResponseAt { get; set; }

        // Motivo de rechazo o de fuerza
        public string? AssignmentComment { get; set; }

        public bool IsArchived { get; set; } = false;

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        [ForeignKey(nameof(ClientContactId))]
        public virtual ContactLog ClientContact { get; set; } = null!;

        [ForeignKey(nameof(AssignedAgentId))]
        public virtual User? AssignedAgent { get; set; }

        [ForeignKey(nameof(AssignedByUserId))]
        public virtual User? AssignedByUser { get; set; }

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

        public virtual ICollection<ConversationHistoryLog> ConversationHistoryLogs { get; set; } = new List<ConversationHistoryLog>();


        [Column(TypeName = "nvarchar(max)")]
        public List<string> Tags { get; set; } = new List<string>();

        [NotMapped]
        public int TotalMessages => Messages.Count();

        [NotMapped]
        public DateTime LastActivity =>
            Messages.Any()
                ? Messages.Max(m => m.SentAt.UtcDateTime)
                : CreatedAt;

        [NotMapped]
        public TimeSpan? Duration
        {
            get
            {
                var nicaraguaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Managua");
                var nowNic = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, nicaraguaTimeZone);
                DateTime reference = ClosedAt.HasValue
                    ? TimeZoneInfo.ConvertTimeFromUtc(ClosedAt.Value.ToUniversalTime(), nicaraguaTimeZone)
                    : nowNic;
                var createdNic = TimeZoneInfo.ConvertTimeFromUtc(CreatedAt.ToUniversalTime(), nicaraguaTimeZone);

                var time = ClosedAt.HasValue ? ClosedAt : reference;
                return CreatedAt - time;
            }
        }

        [NotMapped]
        public TimeSpan? TimeToFirstResponse =>
            FirstResponseAt.HasValue
                ? FirstResponseAt.Value - CreatedAt
                : null;

        [NotMapped]
        public bool IsClosed => Status == ConversationStatus.Closed;
    }
}