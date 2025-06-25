using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class OpeningHourRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public RecurrenceType Recurrence { get; set; }
        public DayOfWeek[]? DaysOfWeek { get; set; }
        public bool? IsWorkShift { get; set; }
        public DayMonth? HolidayDate { get; set; }
        public DateOnly? SpecificDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public DateOnly? EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public bool? IsHolidayMoved { get; set; }
        public DateOnly? HolidayMovedFrom { get; set; }

        public DateOnly? HolidayMoveTo;
        public bool IsActive { get; set; } = true;
    }
}