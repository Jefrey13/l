﻿namespace CustomerService.API.Utils.Enums
{
    public enum AssignmentState
    {
        None = 0,              // Sin solicitud (estado normal)
        Pending = 1,           // El support debe aceptar/rechazar
        Accepted = 2,          // Ha aceptado
        Rejected = 3,          // Ha rechazado
        Forced = 4,             // El admin forzó la asignación
        Unassigned = 5,  // No hay agente asignado
        Assigned =7, // Asignada a un agente
        Reassigned = 9, // Reasignada a otro agente
    }
}
