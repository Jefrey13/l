using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;

namespace CustomerService.API.Services.Implementations
{
    public class PresenceService : IPresenceService
    {
        private readonly ConcurrentDictionary<int, DateTime> _lastOnline = new();
        private readonly IUnitOfWork _uow;

        public PresenceService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task UserConnectedAsync(int userId, CancellationToken cancellation = default)
        {
            var now = DateTime.UtcNow;
            _lastOnline[userId] = now;

            var user = await _uow.Users.GetByIdAsync(userId, cancellation)
                       ?? throw new KeyNotFoundException($"User {userId} not found");
            user.LastOnline = now;
            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task UserDisconnectedAsync(int userId, CancellationToken cancellation = default)
        {
            _lastOnline.TryRemove(userId, out _);
            var now = DateTime.UtcNow;

            var user = await _uow.Users.GetByIdAsync(userId, cancellation)
                       ?? throw new KeyNotFoundException($"User {userId} not found");
            user.LastOnline = now;
            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(cancellation);
        }

        public Task<DateTime?> GetLastOnlineAsync(int userId, CancellationToken cancellation = default)
        {
            _lastOnline.TryGetValue(userId, out var dt);
            return Task.FromResult<DateTime?>(dt);
        }

        public Task<IDictionary<int, DateTime?>> GetLastOnlineAsync(IEnumerable<int> userIds, CancellationToken cancellation = default)
        {
            var result = userIds
                .Distinct()
                .ToDictionary(
                    id => id,
                    id => _lastOnline.TryGetValue(id, out var dt)
                          ? (DateTime?)dt
                          : null
                );
            return Task.FromResult<IDictionary<int, DateTime?>>(result);
        }
    }
}