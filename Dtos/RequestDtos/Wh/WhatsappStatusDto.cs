using System.Text.Json.Serialization;

namespace CustomerService.API.Dtos.RequestDtos.Wh
{
    public class WhatsappStatusDto
    {
        [JsonPropertyName("id")]
        public string MessageId { get; set; } = null!;    // coincide con ExternalId

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;       // e.g. "delivered", "read"

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }               // epoch seconds

        [JsonPropertyName("recipient_id")]
        public string RecipientId { get; set; } = null!;
    }
}