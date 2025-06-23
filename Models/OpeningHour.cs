using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Models
{
    public class OpeningHour
    {
        public int Id { get; set; }                     // PK
        public string Name { get; set; } = string.Empty; // Label
        public string? Description { get; set; }         // Detalles

        // Tipo de recurrencia: semanal, feriado anual o fecha única
        public RecurrenceType Recurrence { get; set; }

        // Días de la semana (para Recurrence.Weekly)
        public DayOfWeek[]? DaysOfWeek { get; set; }

        // Feriado anual: mes y día
        public DayMonth? HolidayDate { get; set; }

        // Feriado puntual: fecha completa
        public DateOnly? SpecificDate { get; set; }

        // Horas de apertura
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Vigencia opcional
        public DateOnly? EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }

        public bool? IsActive { get; set; }             // Estado activo

        // Auditoría
        public int? CreatedById { get; set; }
        public int? UpdatedById { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual User? CreatedBy { get; set; }
        public virtual User? UpdatedBy { get; set; }

        public virtual IEnumerable<WorkShift_User> WorkShift_Users { get; set; }
            = new List<WorkShift_User>();
    }
}