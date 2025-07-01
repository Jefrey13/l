namespace CustomerService.API.Utils.Enums
{
    public enum MessageStatus
    {
        Sent,           // Mensaje enviadoAdd commentMore actions
        Queued,         // En cola en la API de WhatsApp
        Delivered,      // Mensaje entregado al dispositivo
        Read,           // Mensaje marcado como leídoAdd commentMore actions
        Undelivered,    // Mensaje no entregado
        Failed          // Error al enviar o procesar
    }
}
