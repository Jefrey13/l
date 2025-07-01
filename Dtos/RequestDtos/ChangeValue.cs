using CustomerService.API.Dtos.RequestDtos.Wh;
using System.Text.Json.Serialization;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class ChangeValue
    {
        public List<WARequestMessage>? Messages { get; set; } = new();
        public List<Contact>? Contacts { get; set; } = new();

        [JsonPropertyName("statuses")]
        public List<WhatsappStatusDto>? Statuses { get; set; }
    }
}