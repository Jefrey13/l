using System.Text.Json.Serialization;

namespace CustomerService.API.Dtos.RequestDtos.Wh
{
    public class WhatsAppSendResponse
    {
        [JsonPropertyName("messages")]
        public List<WhatsAppMessage> Messages { get; set; } = new();
        public class WhatsAppMessage
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
        }
    }
}
