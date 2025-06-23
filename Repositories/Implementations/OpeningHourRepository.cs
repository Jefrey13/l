using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Utils.Enums;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class OpeningHourRepository: GenericRepository<OpeningHour>, IOpeningHourRepository
    {
        public OpeningHourRepository(CustomerSupportContext context) : base(context) { }

        public async Task<bool> IsHolidayAsync(DateOnly date, CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(oh => oh.IsActive == true
                    && (oh.Recurrence == RecurrenceType.OneTimeHoliday && oh.SpecificDate == date
                        || oh.Recurrence == RecurrenceType.AnnualHoliday
                            && oh.HolidayDate.Month == date.Month
                            && oh.HolidayDate.Day == date.Day)
                    && (oh.EffectiveFrom == null || oh.EffectiveFrom <= date)
                    && (oh.EffectiveTo == null || oh.EffectiveTo >= date))
                .AnyAsync(ct);
        }

        public async Task<bool> IsOutOfOpeningHourAsync(DateTime instant, CancellationToken ct = default)
        {
            var date = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(instant, "America/Managua"));
            var time = TimeOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(instant, "America/Managua"));
            var applicable = await _dbSet
                .AsNoTracking()
                .Where(oh => oh.IsActive == true
                    && (oh.EffectiveFrom == null || oh.EffectiveFrom <= date)
                    && (oh.EffectiveTo == null || oh.EffectiveTo >= date)
                    && (oh.Recurrence == RecurrenceType.Weekly && oh.DaysOfWeek.Contains(date.DayOfWeek)
                        || oh.Recurrence == RecurrenceType.AnnualHoliday
                            && oh.HolidayDate.Month == date.Month
                            && oh.HolidayDate.Day == date.Day
                        || oh.Recurrence == RecurrenceType.OneTimeHoliday && oh.SpecificDate == date))
                .ToListAsync(ct);
            return !applicable.Any(r => r.StartTime <= time && r.EndTime >= time);
        }

        public async Task<IEnumerable<OpeningHour>> GetEffectiveScheduleAsync(DateOnly date, CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(oh => oh.IsActive == true
                    && (oh.EffectiveFrom == null || oh.EffectiveFrom <= date)
                    && (oh.EffectiveTo == null || oh.EffectiveTo >= date)
                    && (oh.Recurrence == RecurrenceType.Weekly && oh.DaysOfWeek.Contains(date.DayOfWeek)
                        || oh.Recurrence == RecurrenceType.AnnualHoliday
                            && oh.HolidayDate.Month == date.Month
                            && oh.HolidayDate.Day == date.Day
                        || oh.Recurrence == RecurrenceType.OneTimeHoliday && oh.SpecificDate == date))
                .ToListAsync(ct);
        }
    }
}