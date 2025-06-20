using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class OpeningHourRepository: GenericRepository<OpeningHour>, IOpeningHourRepository
    {
        public OpeningHourRepository(CustomerSupportContext context) : base(context) { }

       public async Task<bool> IsHolidayAsync(CancellationToken ct = default)
        {
            var today = DateTime.Today;
            var todayString = today.ToString("dd/MM");

            return await _dbSet.AsNoTracking()
                .Where(h => h.IsHoliday == true && h.IsActive == true)
                .ToListAsync(ct)
                .ContinueWith(task => task.Result.Any(h => h.HolidayDate.ToString() == todayString), ct);
        }

        public async Task<bool> IsOutOfOpeningHour(CancellationToken ct = default)
        {
            var localTime = TimeOnly.FromDateTime(DateTime.Now);

            //Si la hora actual es antes de la hora de inicio y despues de la hora final se retorna false, si esta dentro del rango se retorna true.
            //eje: si la jhora actual es la 1:00 y la hora de incio 8:00 y cierre 15:00 retorna true esta fuera del rango, o si son las 16:00. Pero si es la 14:00 esta ddentro del rango y retorna false
            //Cancelationtoken ct
            var isInRange = await _dbSet
                .AsNoTracking()
                .AnyAsync(h => h.StartTime.HasValue && h.EndTime.HasValue &&
                               h.StartTime.Value <= localTime || localTime >= h.EndTime.Value, ct);

            return isInRange;
        }
    }
}