using CustomerService.API.Models;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByTypeAsync(
            NotificationType type,
            CancellationToken cancellation = default);

        Task<Notification?> GetWithRecipientsAsync(
            int notificationId,
            CancellationToken cancellation = default);
    }
}
