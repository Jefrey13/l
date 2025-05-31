using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(CustomerSupportContext context) : base(context) { }

        public async Task<IEnumerable<Message>> GetByConversationAsync(int conversationId, CancellationToken cancellation = default)
        {
            if (conversationId <= 0)
                throw new ArgumentException("El conversationId debe ser mayor que cero.", nameof(conversationId));

            return await _dbSet
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId)
                .Include(m => m.SenderUser)
                .Include(m => m.SenderContact)
                .Include(m => m.Attachments)
                .OrderBy(m => m.DeliveredAt)
                .ToListAsync(cancellation);
        }

        public async Task<Message?> GetByIdNoTrackingAsync(int id, CancellationToken ct = default)
{
    return await _context.Messages
        .AsNoTracking()
        .SingleOrDefaultAsync(m => m.MessageId == id, ct);
}
    }
}