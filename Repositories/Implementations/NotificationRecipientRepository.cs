using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class NotificationRecipientRepository
        : GenericRepository<NotificationRecipient>, INotificationRecipientRepository
    {
        public NotificationRecipientRepository(CustomerSupportContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<NotificationRecipient>> GetByUserAsync(
            int userId,
            CancellationToken cancellation = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(nr => nr.UserId == userId)
                .Include(nr => nr.Notification)
                .ToListAsync(cancellation);
        }

        public async Task<int> GetUnreadCountAsync(
            int userId,
            CancellationToken cancellation = default)
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(nr => nr.UserId == userId && !nr.IsRead, cancellation);
        }
    }
}
