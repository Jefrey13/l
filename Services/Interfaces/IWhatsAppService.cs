using CustomerService.API.Dtos.RequestDtos;

namespace CustomerService.API.Services.Interfaces
{
    /// <summary>
    /// Envía mensajes de texto y media a través de la WhatsApp Cloud API.
    /// </summary>
    public interface IWhatsAppService
    {
        /// <summary>Envía texto por WhatsApp.</summary>
        Task SendTextAsync(int conversationId, int senderId, string text, CancellationToken cancellation = default);

        /// <summary>Envía una lista de botones interactivos.</summary>
        Task SendInteractiveButtonsAsync(int conversationId, int senderId, string header, IEnumerable<WhatsAppInteractiveButton> buttons, CancellationToken cancellation = default);

        /// <summary>Sube un archivo y devuelve su mediaId.</summary>
        Task<string> UploadMediaAsync(byte[] data, string mimeType, string fileName = "file", CancellationToken cancellation = default);

        /// <summary>Envía un medio ya subido (imagen, video, audio, sticker o documento).</summary>
        Task SendMediaAsync(int conversationId, int senderId, string mediaId, string mimeType, string? caption = null, CancellationToken cancellation = default);

        /// <summary>Obtiene la URL temporal de descarga para un mediaId.</summary>
        Task<string> DownloadMediaUrlAsync(string mediaId, CancellationToken cancellation = default);

        /// <summary>Descarga el contenido binario de una URL.</summary>
        Task<byte[]> DownloadMediaAsync(string mediaUrl, CancellationToken cancellation = default);

    }
}
