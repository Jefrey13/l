using CustomerService.API.Models;
using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IConversationRepository : IGenericRepository<Conversation>
    {
        Task<IEnumerable<Conversation>> GetPendingAsync(CancellationToken cancellation = default);

        Task<IEnumerable<Conversation>> GetByAgentAsync(int agentId, CancellationToken cancellation = default);

        Task<int> CountAssignedAsync(int agentId, CancellationToken cancellation = default);

        //Task<Conversation> GetByStatusAsync(List<ConversationStatus> statuses, CancellationToken cancellation = default);
    }
}