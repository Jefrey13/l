using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IOpeningHourRepository: IGenericRepository<OpeningHour>
    {
        Task<IQueryable<OpeningHour>> GetAllAsync();
        Task<bool> IsHolidayAsync(DateOnly date, CancellationToken ct = default);
        Task<bool> IsOutOfOpeningHourAsync(DateTime instant, CancellationToken ct = default);
        Task<IEnumerable<OpeningHour>> GetEffectiveScheduleAsync(DateOnly date, CancellationToken ct = default);
    }
}
