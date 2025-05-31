using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<IEnumerable<Message>> GetByConversationAsync(int conversationId, CancellationToken cancellation = default);

        Task<Message?> GetByIdNoTrackingAsync(int id, CancellationToken ct = default);
    }
}
