namespace CustomerService.API.Utils.Enums
{
    /// <summary>
    /// Estado de validación de un contacto en el log.
    /// </summary>
    public enum ContactStatus
    {
        New,                // recién creado, sin datos
        PendingApproval,    // aprobación manual
        Approved,           // aprobado completamente
        Rejected,           // rechazado manualmente
        AwaitingFullName,   // nombre completo
        AwaitingIdCard,     // número de cédula
        AwaitingCompanyName, //nombre de la empresa del contacto
        Completed           // ya recibimos ambos datos
    }
}
