namespace CustomerService.API.Utils.Enums
{
    public enum AssignmentState
    {
        Unassigned,  // No hay agente asignado
        Pending, // Asignación pendiente
        Assigned, // Asignada a un agente
        Rejected, // Rechazada por el agente
        Reassigned, // Reasignada a otro agente
    }
}
