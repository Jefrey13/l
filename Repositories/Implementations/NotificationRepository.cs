using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Utils.Enums;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class NotificationRepository
       : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(CustomerSupportContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetByTypeAsync(
            NotificationType type,
            CancellationToken cancellation = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(n => n.Type == type)
                .Include(n => n.Recipients)
                .ToListAsync(cancellation);
        }

        public async Task<Notification?> GetWithRecipientsAsync(
            int notificationId,
            CancellationToken cancellation = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(n => n.Recipients)
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId, cancellation);
        }

        //public async Task<Notification> AddNotificationAsync(Notification entity, CancellationToken cancellation = default)
        //{
        //    try
        //    {
        //        if (entity == null)
        //            throw new ArgumentNullException(nameof(entity));

        //        var newNotification = await _dbSet.AddAsync(entity, CancellationToken.None);
        //        await _dbSet(cancellation);

        //        return newNotification.Entity;
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //        return new Notification();
        //    }
        //}
    }
}
