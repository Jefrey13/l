using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IContactLogRepository: IGenericRepository<ContactLog>
    {
        Task<ContactLog?> GetByPhone(string phoneNumber, CancellationToken cancellation = default);
    }
}