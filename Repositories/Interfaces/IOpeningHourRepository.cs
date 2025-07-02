using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IOpeningHourRepository: IGenericRepository<OpeningHour>
    {
        Task<bool> IsHolidayAsync(DateOnly date, CancellationToken ct = default);
        Task<bool> IsThereWorkShiftAsync(DateTime instant, CancellationToken ct = default);
        Task<IEnumerable<OpeningHour>> GetEffectiveScheduleAsync(DateOnly date, CancellationToken ct = default);
    }
}
