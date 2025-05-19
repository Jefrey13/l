using System.Text.Json.Serialization;

namespace CustomerService.API.Dtos.RequestDtos.Wh
{
    public class InteractiveDto
    {
        // public ButtonReplyDto? ButtonReply { get; set; }

        [JsonPropertyName("list_reply")]
        public ListReplyDto? ListReply { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
