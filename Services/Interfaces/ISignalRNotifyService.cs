using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;

namespace CustomerService.API.Services.Interfaces
{
    public interface ISignalRNotifyService
    {
        Task NotifyNewContactCreatedAsync(NewContactCreatedDto dto, CancellationToken cancellation = default);
        Task NotifySupportRequestedAsync(SupportRequestedDto dto, CancellationToken cancellation = default);
        Task NotifyConversationAssignedAsync(ConversationAssignedDto dto, CancellationToken cancellation = default);

        Task SendNotificationToUsersAsync(IEnumerable<int> userIds, NotificationResponseDto dto, CancellationToken cancellation = default);

        Task NotifyUserAsync(int userId, string method, object payload, CancellationToken cancellation = default);
    }
}