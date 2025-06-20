using CustomerService.API.Utils;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class OpeningHourRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Horarios específicos (solo hora, sin fecha completa)
        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public bool? IsHoliday { get; set; }

        public DayMonth? HolidayDate { get; set; }

        public bool? IsActive { get; set; }
    }
}