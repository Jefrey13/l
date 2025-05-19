using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface INotificationRecipientRepository
        : IGenericRepository<NotificationRecipient>
    {
        Task<IEnumerable<NotificationRecipient>> GetByUserAsync(
            int userId,
            CancellationToken cancellation = default);

        Task<int> GetUnreadCountAsync(
            int userId,
            CancellationToken cancellation = default);
    }
}
