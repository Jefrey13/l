using System.Text.Json.Serialization;
using CustomerService.API.Dtos.RequestDtos.Wh;
using Microsoft.AspNetCore.Identity;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class WARequestMessage
    {
        public string From { get; set; } = null!;
        public TextDto? Text { get; set; }
        public ImageDto? Image { get; set; }
        public VideoDto? Video { get; set; }

        public AudioDto? Audio { get; set; }

        public StickerDto? Sticker { get; set; }
        public DocumentDto? Document { get; set; }
        public string? Caption { get; set; }
        public string MessageId { get; set; } = "";

        [JsonPropertyName("interactive")]
        public InteractiveDto? Interactive { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}