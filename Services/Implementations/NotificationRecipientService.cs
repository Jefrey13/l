using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Services.Implementations
{
    public class NotificationRecipientService : INotificationRecipientService
    {
        private readonly INotificationRecipientRepository _repo;
        private readonly IUnitOfWork _uow;

        public NotificationRecipientService(
            INotificationRecipientRepository repo,
            IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<PagedResponse<NotificationResponseDto>> GetByUserAsync(
            PaginationParams @params,
            int userId,
            CancellationToken cancellation = default)
        {
            var query = _repo.GetAll()
                             .Where(nr => nr.UserId == userId)
                             .Include(nr => nr.Notification)
                             .OrderByDescending(nr => nr.Notification.CreatedAt);

            var paged = await PagedList<NotificationRecipient>.CreateAsync(
                query,
                @params.PageNumber,
                @params.PageSize,
                cancellation);

            var dtos = paged.Select(nr => nr.Adapt<NotificationResponseDto>()).ToList();
            return new PagedResponse<NotificationResponseDto>(dtos, paged.MetaData);
        }

        public async Task<PagedResponse<NotificationResponseDto>> GetUnreadByUserAsync(
            PaginationParams @params,
            int userId,
            CancellationToken cancellation = default)
        {
            var query = _repo.GetAll()
                             .Where(nr => nr.UserId == userId && !nr.IsRead)
                             .Include(nr => nr.Notification)
                             .OrderByDescending(nr => nr.Notification.CreatedAt);

            var paged = await PagedList<NotificationRecipient>.CreateAsync(
                query,
                @params.PageNumber,
                @params.PageSize,
                cancellation);

            var dtos = paged.Select(nr => nr.Adapt<NotificationResponseDto>()).ToList();
            return new PagedResponse<NotificationResponseDto>(dtos, paged.MetaData);
        }

        public Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellation = default)
        {
            return _repo.GetUnreadCountAsync(userId, cancellation);
        }

        public async Task MarkAsReadAsync(int notificationRecipientId, CancellationToken cancellation = default)
        {
            var entity = await _repo.GetByIdAsync(notificationRecipientId, cancellation)
                         ?? throw new KeyNotFoundException($"NotificationRecipient {notificationRecipientId} not found");
            entity.IsRead = true;
            entity.ReadAt = DateTime.UtcNow;
            _repo.Update(entity);
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task MarkAllReadAsync(int userId, CancellationToken cancellation = default)
        {
            var unread = await _repo.GetAll()
                .Where(nr => nr.UserId == userId && !nr.IsRead)
                .ToListAsync(cancellation);

            if (!unread.Any()) return;

            var now = DateTime.UtcNow;
            foreach (var nr in unread)
            {
                nr.IsRead = true;
                nr.ReadAt = now;
                _repo.Update(nr);
            }

            await _uow.SaveChangesAsync(cancellation);
        }
    }
}