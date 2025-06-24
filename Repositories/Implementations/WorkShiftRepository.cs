using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Utils.Enums;
using Microsoft.EntityFrameworkCore;
using WhatsappBusiness.CloudApi.Webhook;

namespace CustomerService.API.Repositories.Implementations
{
    public class WorkShiftRepository : GenericRepository<WorkShift_User>, IWorkShiftRepository
    {
        public WorkShiftRepository(CustomerSupportContext context) : base(context) { }

        public virtual IQueryable<WorkShift_User> GetAll() =>
           _dbSet.AsNoTracking().Include(ws=> ws.AssignedUser).Include(ws=> ws.CreatedBy).Include(ws=> ws.CreatedBy).Include(ws=> ws.OpeningHour);

        public override async Task<WorkShift_User> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _dbSet
                .Include(ws => ws.AssignedUser)
                .Include(ws => ws.CreatedBy)
                .Include(ws => ws.UpdatedBy)
                .Include(ws => ws.OpeningHour)
                .FirstOrDefaultAsync(ws => ws.Id == id, ct);
            if (entity == null) throw new KeyNotFoundException($"WorkShift_User with ID {id} not found.");
            return entity;
        }

        public async Task<int> GetActiveAssignmentsCountAsync(DateOnly date, CancellationToken ct = default) =>
            await _dbSet
                .AsNoTracking()
                .CountAsync(ws =>
                    ws.IsActive
                    && (ws.ValidFrom == null || ws.ValidFrom <= date)
                    && (ws.ValidTo == null || ws.ValidTo >= date),
                ct);

        public async Task<IEnumerable<WorkShift_User>> GetByDateAsync(DateOnly date, CancellationToken ct = default) =>
            await _dbSet
                .AsNoTracking()
                .Include(ws => ws.AssignedUser)
                .Include(ws => ws.OpeningHour)
                .Where(ws =>
                    ws.IsActive
                    && (ws.ValidFrom == null || ws.ValidFrom <= date)
                    && (ws.ValidTo == null || ws.ValidTo >= date))
                .ToListAsync(ct);

        public async Task<IEnumerable<User>> GetMembersOnShiftAsync(DateTime instant, CancellationToken ct = default)
        {
            try
            {
                var local = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(instant, "America/Managua");
                var date = DateOnly.FromDateTime(local);
                var time = TimeOnly.FromDateTime(local);

                var shifts = await _dbSet
                    .AsNoTracking()
                    .Include(ws => ws.AssignedUser)
                    .Include(ws => ws.OpeningHour)
                    .Where(ws =>
                        ws.IsActive &&
                        (
                            // Solo los Weekly validan rango de WorkShift
                            (ws.OpeningHour.Recurrence == RecurrenceType.Weekly
                                && (ws.ValidFrom == null || ws.ValidFrom <= date)
                                && (ws.ValidTo == null || ws.ValidTo >= date)
                            )
                            || ws.OpeningHour.Recurrence == RecurrenceType.OneTimeHoliday
                            || ws.OpeningHour.Recurrence == RecurrenceType.AnnualHoliday
                        )
                    )
                    .ToListAsync(ct);

                // ahora filtras en memoria exactamente igual que antes
                var result = shifts.Where(ws =>
                    (ws.OpeningHour.Recurrence == RecurrenceType.Weekly &&
                        ws.OpeningHour.DaysOfWeek?.Contains(date.DayOfWeek) == true
                        && ws.OpeningHour.StartTime <= time
                        && time <= ws.OpeningHour.EndTime
                    )
                    ||
                    (ws.OpeningHour.Recurrence == RecurrenceType.OneTimeHoliday &&
                        (
                            (ws.OpeningHour.SpecificDate.HasValue && ws.OpeningHour.SpecificDate.Value == date)
                            || (!ws.OpeningHour.SpecificDate.HasValue
                                && ws.OpeningHour.EffectiveFrom <= date
                                && ws.OpeningHour.EffectiveTo >= date)
                        )
                    )
                    ||
                    (ws.OpeningHour.Recurrence == RecurrenceType.AnnualHoliday &&
                        ws.OpeningHour.HolidayDate.Month == date.Month
                        && ws.OpeningHour.HolidayDate.Day == date.Day
                    )
                ).ToList();

                // extrae usuarios únicos
                return result
                    .Select(ws => ws.AssignedUser!)
                    .GroupBy(u => u.UserId)
                    .Select(g => g.First());
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return null;
            }
        }

    }
}