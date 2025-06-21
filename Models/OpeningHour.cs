using CustomerService.API.Utils;

namespace CustomerService.API.Models
{
    public class OpeningHour
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Horarios específicos (solo hora, sin fecha completa)
        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public DayMonth? HolidayDate { get; set; }

        public bool? IsHoliday { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedById { get; set; }

        public int? UpdatedById { get; set; }

        public virtual User? CreatedBy { get; set; } = null!;

        public virtual User? UpdatedBy { get; set; } = null!;

        public virtual IEnumerable<WorkShift_User> WorkShift_Users { get; set; } = new List<WorkShift_User>();
    }
}