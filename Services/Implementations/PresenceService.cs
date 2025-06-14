using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using WhatsappBusiness.CloudApi.Messages.Requests;

namespace CustomerService.API.Services.Implementations
{
    public class PresenceService : IPresenceService
    {
        private readonly ConcurrentDictionary<int, DateTime> _lastOnline = new();
        private readonly IUnitOfWork _uow;
        private readonly INicDatetime _nicDatetime;

        public PresenceService(IUnitOfWork uow,
            INicDatetime nicDatetime)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _nicDatetime = nicDatetime;
        }

        public Task<bool> IsUserConnectedAsync(int userId, CancellationToken cancellation = default)
        {
            // Si la clave existe, la conexión SignalR sigue activa
            return Task.FromResult(_lastOnline.ContainsKey(userId));
        }
        public async Task UserConnectedAsync(int userId, CancellationToken cancellation = default)
        {
            var now = await _nicDatetime.GetNicDatetime();

            _lastOnline[userId] = now;

            var user = await _uow.Users.GetByIdAsync(userId, cancellation)
                       ?? throw new KeyNotFoundException($"User {userId} not found");

            user.LastOnline = now;
            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task UserDisconnectedAsync(int userId, CancellationToken cancellation = default)
        {
            try
            {
                _lastOnline.TryRemove(userId, out _);

                await _uow.Users.ClearLastOnlineAsync(userId, cancellation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<DateTime?> GetLastOnlineAsync(int userId, CancellationToken cancellation = default)
        {
            // 1. Primero intenta obtenerlo desde memoria
            if (_lastOnline.TryGetValue(userId, out var dt))
                return dt;

            // 2. Si no está en memoria, consulta la base de datos
            var user = await _uow.Users.GetByIdAsync(userId, cancellation);

            return user?.LastOnline;
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