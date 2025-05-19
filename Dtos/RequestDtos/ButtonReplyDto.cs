using System.Text.Json.Serialization;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class ButtonReplyDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
    }
}
