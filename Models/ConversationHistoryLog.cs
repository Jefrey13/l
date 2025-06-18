using CustomerService.API.Utils.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerService.API.Models
{
    /// <summary>
    /// Registro histórico de cambios de estado de una conversación.
    /// </summary>
    public class ConversationHistoryLog
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }

        public ConversationStatus OldStatus { get; set; }
        public ConversationStatus NewStatus { get; set; }

        public int? ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(45)]
        public string? SourceIp { get; set; }

        [MaxLength(200)]
        public string? UserAgent { get; set; }
 
        [ForeignKey(nameof(ConversationId))]
        public virtual Conversation Conversation { get; set; } = null!;

        [ForeignKey(nameof(ChangedByUserId))]
        public virtual User? ChangedByUser { get; set; } = null!;
    }
}