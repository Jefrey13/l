using System.Text.Json.Serialization;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class ListReplyDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
    }
}
