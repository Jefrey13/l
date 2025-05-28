using System.Text.Json.Serialization;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class ImageDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; } = null!;

        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; } = null!;
    }
}