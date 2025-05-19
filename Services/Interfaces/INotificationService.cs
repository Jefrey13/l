using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateAsync(NotificationType type, string payload, IEnumerable<int> recipientUserIds, CancellationToken cancellation = default);
    }
}