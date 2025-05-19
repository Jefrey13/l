using WhatsappBusiness.CloudApi.Webhook;

namespace CustomerService.API.Dtos.RequestDtos.Wh
{
    public class StatusChange
    {
        public StatusValue Value { get; set; } = new();
    }
}
