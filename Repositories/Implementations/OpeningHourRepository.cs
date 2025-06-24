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
                .Where(oh =>
                    oh.IsActive == true
                    && (
                        // Feriado puntual o rango
                        (oh.Recurrence == RecurrenceType.OneTimeHoliday
                            && (
                                (oh.SpecificDate.HasValue && oh.SpecificDate.Value == date)
                                || (!oh.SpecificDate.HasValue
                                    && oh.EffectiveFrom.HasValue && oh.EffectiveFrom.Value <= date
                                    && oh.EffectiveTo.HasValue && oh.EffectiveTo.Value >= date)
                            )
                        )
                        ||
                        // Feriado anual
                        (oh.Recurrence == RecurrenceType.AnnualHoliday
                            && oh.HolidayDate != null
                            && oh.HolidayDate.Month == date.Month
                            && oh.HolidayDate.Day == date.Day
                        )
                    )
                )
                .AnyAsync(ct);
        }

        public async Task<bool> IsOutOfOpeningHourAsync(DateTime instant, CancellationToken ct = default)
        {
            var local = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(instant, "America/Managua");
            var date = DateOnly.FromDateTime(local);
            var time = TimeOnly.FromDateTime(local);

            var weeklySlots = await _dbSet

                .AsNoTracking()
                .Where(oh =>
                    oh.IsActive == true
                    && oh.Recurrence == RecurrenceType.Weekly
                    && oh.DaysOfWeek != null
                    && oh.DaysOfWeek.Contains(date.DayOfWeek)
                )
                .ToListAsync(ct);

            var inside = weeklySlots.Any(r => r.StartTime <= time && time <= r.EndTime);
            return !inside;
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