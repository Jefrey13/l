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
        public DateTime? AssignedAt { get; set; }

        public ConversationStatus Status { get; set; } = ConversationStatus.New;
        public bool Initialized { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FirstResponseAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
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

        /// <summary>
        /// Lista de etiquetas asociadas a la conversación,
        /// serializada como JSON en la base de datos.
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public List<string> Tags { get; set; } = new List<string>();

        // Campos calculados (no mapeados a la BD)
        [NotMapped]
        public int TotalMessages => Messages.Count;

        [NotMapped]
        public DateTime LastActivity =>
            Messages.Any()
                ? Messages.Max(m => m.SentAt.UtcDateTime)
                : CreatedAt;

        [NotMapped]
        public TimeSpan Duration =>
            (ClosedAt ?? DateTime.UtcNow) - CreatedAt;

        [NotMapped]
        public TimeSpan? TimeToFirstResponse =>
            FirstResponseAt.HasValue
                ? FirstResponseAt - CreatedAt
                : null;

        [NotMapped]
        public bool IsClosed => Status == ConversationStatus.Closed;
    }
}