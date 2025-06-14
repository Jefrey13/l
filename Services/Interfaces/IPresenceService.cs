using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerService.API.Services.Interfaces
{
    public interface IPresenceService
    {
        // Marca conexión/desconexión
        Task UserConnectedAsync(int userId, CancellationToken cancellation = default);
        Task UserDisconnectedAsync(int userId, CancellationToken cancellation = default);

        // Consultas de última conexión
        Task<DateTime?> GetLastOnlineAsync(int userId, CancellationToken cancellation = default);
        Task<IDictionary<int, DateTime?>> GetLastOnlineAsync(IEnumerable<int> userIds, CancellationToken cancellation = default);
        Task<bool> IsUserConnectedAsync(int userId, CancellationToken cancellation = default);
    }
}