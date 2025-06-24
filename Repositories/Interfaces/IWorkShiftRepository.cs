using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IWorkShiftRepository : IGenericRepository<WorkShift_User>
    {
        Task<int> GetActiveAssignmentsCountAsync(DateOnly date, CancellationToken ct = default);
        Task<IEnumerable<WorkShift_User>> GetByDateAsync(DateOnly date, CancellationToken ct = default);

        Task<IEnumerable<User>> GetMembersOnShiftAsync(DateTime instant, CancellationToken ct = default);
    }
}
