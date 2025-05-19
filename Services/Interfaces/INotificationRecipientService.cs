using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Utils;

namespace CustomerService.API.Services.Interfaces
{
    public interface INotificationRecipientService
    {
        Task<PagedResponse<NotificationDto>> GetByUserAsync(PaginationParams @params, int userId, CancellationToken cancellation = default);
        Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellation = default);
        Task MarkAsReadAsync(int notificationRecipientId, CancellationToken cancellation = default);
    }
}