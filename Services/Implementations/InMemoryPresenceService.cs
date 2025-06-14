using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Services.Interfaces;

namespace CustomerService.API.Services.Implementations
{
    public class InMemoryPresenceService : IPresenceService
    {
        private readonly ConcurrentDictionary<int, DateTime> _lastOnline = new();

        public Task UserConnectedAsync(int userId, CancellationToken cancellation = default)
        {
            _lastOnline[userId] = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public Task UserDisconnectedAsync(int userId, CancellationToken cancellation = default)
        {
            _lastOnline.TryRemove(userId, out _);
            return Task.CompletedTask;
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

        public Task<bool> IsUserConnectedAsync(int userId, CancellationToken cancellation = default)
        {
            // Si la clave existe, la conexión SignalR sigue activa
            return Task.FromResult(_lastOnline.ContainsKey(userId));
        }
    }
}