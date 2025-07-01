using CustomerService.API.Utils.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CustomerService.API.Models
{
    public class MessageStatusHistory
    {
        [Key]
        public int Id { get; set; } // coincide con ExternalId

        [Required]
        public int MessageId { get; set; }

        [Required]
        public MessageStatus Status { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// JSON o texto libre con detalles del webhook, código de error, respuesta de la API, etc.
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? Metadata { get; set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; set; } = null!;
    }
}