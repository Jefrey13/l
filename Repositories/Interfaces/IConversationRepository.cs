using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IConversationRepository : IGenericRepository<Conversation>
    {
        Task<IEnumerable<Conversation>> GetPendingAsync(CancellationToken cancellation = default);

        Task<IEnumerable<Conversation>> GetByAgentAsync(int agentId, CancellationToken cancellation = default);

        Task<int> CountAssignedAsync(int agentId, CancellationToken cancellation = default);

        IQueryable<Conversation> QueryByState(string status);

        Task<IEnumerable<ConversationStatusCountResponseDto>> GetConversationsCountByDateRange(FilterDashboard filters, CancellationToken ct = default);

        Task<IEnumerable<AverageAssignmentTimeResponseDto>> AverageAssignmentTimeAsync(FilterDashboard filters, CancellationToken ct = default);

        Task<IEnumerable<AdminAsigmentResponseTimeResponseDto>> AssigmentResponseTimeAsync(FilterDashboard filters, CancellationToken ct = default);

        Task<IEnumerable<WaitingClientResponseDto>> GetWaitingClient(FilterDashboard filters, int? criticalMinutes, CancellationToken ct = default);

        Task<ResponseAgentAverageResponseDto> ResponseAgentAverageAsync(FilterDashboard filters,CancellationToken ct = default);

        IQueryable<Conversation> GetConversationByClient(FilterDashboard filters,
            CancellationToken ct = default);
        //Task<Conversation> GetByStatusAsync(List<ConversationStatus> statuses, CancellationToken cancellation = default);
    }
}