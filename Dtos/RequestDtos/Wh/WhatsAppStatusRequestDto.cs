using WhatsappBusiness.CloudApi.Webhook;

namespace CustomerService.API.Dtos.RequestDtos.Wh
{
    public class WhatsAppStatusRequestDto
    {
        public List<StatusEntry> Entry { get; set; } = new();
    }
}
