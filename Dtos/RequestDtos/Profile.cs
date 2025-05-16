using System.Text.Json.Serialization;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class Profile
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}