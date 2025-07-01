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
        AwaitingIdType,   // nombre completo
        AwaitingIdCard,     // número de cédula
        AwaitingPassword,   // número de numero de pasaporte
        AwaitingResidenceCard, //Numero de cedula de recidencia
        AwaitingCompanyName, //nombre de la empresa del contacto
        Completed           // ya recibimos ambos datos
    }
}
