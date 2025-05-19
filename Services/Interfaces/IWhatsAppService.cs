using CustomerService.API.Dtos.RequestDtos;

namespace CustomerService.API.Services.Interfaces
{
    /// <summary>
    /// Envía mensajes de texto y media a través de la WhatsApp Cloud API.
    /// </summary>
    public interface IWhatsAppService
    {
        Task SendTextAsync(int conversationId, int senderId, string text, CancellationToken cancellation = default);
        Task<string> UploadMediaAsync(byte[] data, string mimeType);
        Task SendMediaAsync(int conversationId, int senderId, string mediaId, string mimeType, string? caption = null, CancellationToken cancellation = default);
        Task HandleWebhookAsync(WhatsAppWebhookRequestDto webhook, CancellationToken cancellation = default);

        Task SendInteractiveButtonsAsync(
            int conversationId,
            int senderId,
            string header,
            IEnumerable<WhatsAppInteractiveButton> buttons,
            CancellationToken cancellation = default);

    }
}
