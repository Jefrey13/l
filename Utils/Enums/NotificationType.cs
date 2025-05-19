namespace CustomerService.API.Utils.Enums
{
    public enum NotificationType
    {
        NewContact,          // Nuevo ContactLog creado
        SupportRequested,    // Cliente pide soporte humano
        ConversationAssigned, // Conversación asignada a agente
        ConversationClosed
    }
}