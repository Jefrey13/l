using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.RequestDtos
{
    /// <summary>
    /// Representa el mensaje normalizado: origen, texto y, si aplica, ID de interacción.
    /// </summary>
    public class IncomingPayload
    {
        public string? MessageId { get; set; }
        public string From { get; set; } = null!;
        public string? TextBody { get; set; }
        public string? InteractiveId { get; set; }
        public InteractiveType Type { get; set; }
    }
}
