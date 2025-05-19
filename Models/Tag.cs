using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Models
{
    /// <summary>
    /// Etiqueta para clasificar/contexualizar una conversación (p. ej. “soporte”, “nuevo”, “prioritario”, etc.).
    /// </summary>
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        public virtual ICollection<ConversationTag> ConversationTags { get; set; }
            = new List<ConversationTag>();
    }
}
