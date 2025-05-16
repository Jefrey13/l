using System.Text.Json.Serialization;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class Contact
    {
        [JsonPropertyName("wa_id")]
        public string? WaId { get; set; }

        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        public Profile? Profile { get; set; }
    }
}