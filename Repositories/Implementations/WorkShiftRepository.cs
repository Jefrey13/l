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

        /// <summary>
        /// Devuelve la lista de usuarios asignados que están de turno en el instante dado.
        /// </summary>
        public async Task<IEnumerable<User>> GetMembersOnShiftAsync(DateTime instant, CancellationToken ct = default)
        {
            // 1) convertir al huso de Managua
            var local = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(instant, "America/Managua");
            var date = DateOnly.FromDateTime(local);
            var time = TimeOnly.FromDateTime(local);

            // 2) filtrar WorkShift_User válidos en fecha/hora y traer AssignedUser
            var shifts = await _dbSet
                .AsNoTracking()
                .Include(ws => ws.AssignedUser)
                .Include(ws => ws.OpeningHour)
                .Where(ws =>
                    ws.IsActive
                    && (ws.ValidFrom == null || ws.ValidFrom <= date)
                    && (ws.ValidTo == null || ws.ValidTo >= date)

                    // OpeningHour cubre este instante:
                    && (
                        // Semanales
                        (ws.OpeningHour.Recurrence == RecurrenceType.Weekly
                            && ws.OpeningHour.DaysOfWeek != null
                            && ws.OpeningHour.DaysOfWeek.Contains(date.DayOfWeek)
                            && ws.OpeningHour.StartTime <= time
                            && time <= ws.OpeningHour.EndTime
                        )
                        ||
                        // Feriado puntual o rango
                        (ws.OpeningHour.Recurrence == RecurrenceType.OneTimeHoliday
                            && (
                                (ws.OpeningHour.SpecificDate.HasValue && ws.OpeningHour.SpecificDate.Value == date)
                                || (!ws.OpeningHour.SpecificDate.HasValue
                                    && ws.OpeningHour.EffectiveFrom.HasValue && ws.OpeningHour.EffectiveFrom.Value <= date
                                    && ws.OpeningHour.EffectiveTo.HasValue && ws.OpeningHour.EffectiveTo.Value >= date)
                            )
                        )
                        ||
                        // Feriado anual
                        (ws.OpeningHour.Recurrence == RecurrenceType.AnnualHoliday
                            && ws.OpeningHour.HolidayDate != null
                            && ws.OpeningHour.HolidayDate.Month == date.Month
                            && ws.OpeningHour.HolidayDate.Day == date.Day
                        )
                    )
                )
                .ToListAsync(ct);

            // 3) Devolver los usuarios asignados (sin duplicados)
            return shifts
                .Select(ws => ws.AssignedUser)
                .Where(u => u != null)
                .GroupBy(u => u.UserId)
                .Select(g => g.First());
        }
    }
}