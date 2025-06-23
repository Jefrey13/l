using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerService.API.Models
{
    public class WorkShift_User
    {
        public int Id { get; set; }                       // PK
        public int OpeningHourId { get; set; }            // FK a OpeningHour
        public int AssignedUserId { get; set; }           // FK a User

        public int CreatedById { get; set; }              // FK creador
        public int? UpdatedById { get; set; }             // FK actualizador

        public bool IsActive { get; set; }                // Estado activo

        // Vigencia de la asignación
        public DateOnly? ValidFrom { get; set; }          // Desde fecha
        public DateOnly? ValidTo { get; set; }            // Hasta fecha

        public DateTime CreatedAt { get; set; }           // Fecha creación
        public DateTime? UpdatedAt { get; set; }          // Fecha actualización

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;   // Control de concurrencia

        // Navegación
        public virtual OpeningHour OpeningHour { get; set; } = null!;
        public virtual User AssignedUser { get; set; } = null!;
        public virtual User? CreatedBy { get; set; }
        public virtual User? UpdatedBy { get; set; }
    }
}