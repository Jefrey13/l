using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Utils.Enums;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
    {
        public ConversationRepository(CustomerSupportContext context)
            : base(context) { }

        public async Task<IEnumerable<Conversation>> GetPendingAsync(CancellationToken cancellation = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.Status == ConversationStatus.Waiting
                         || c.Status == ConversationStatus.Bot)
                .Include(c => c.Messages)
                .ToListAsync(cancellation);
        }

        public async Task<IEnumerable<Conversation>> GetByAgentAsync(int agentId, CancellationToken cancellation = default)
        {
            if (agentId <= 0) throw new ArgumentException(nameof(agentId));

            return await _dbSet.AsNoTracking()
                .Where(c => c.AssignedAgentId == agentId)
                .Include(c => c.ClientContact)
                .Include(c => c.AssignedAgent)
                .Include(c => c.AssignedByUser)
                .Include(c => c.Messages)
                    .ThenInclude(a=> a.Attachments)
                .ToListAsync(cancellation);
        }

        public async Task<int> CountAssignedAsync(int agentId, CancellationToken cancellation = default)
        {
            if (agentId <= 0) throw new ArgumentException(nameof(agentId));

            return await _dbSet
                .AsNoTracking()
                .Where(c =>
                    c.AssignedAgentId == agentId
                    && c.Status == ConversationStatus.Human
                    && !c.IsArchived
                )
                .CountAsync(cancellation);
        }

        public override IQueryable<Conversation> GetAll()
            => _dbSet
                .Include(c => c.ClientContact)
                .Include(c => c.AssignedAgent)
                .Include(c => c.Messages)
                        .ThenInclude(a=>  a.Attachments);

        public override async Task<Conversation?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var conversation = await _dbSet
                .Where(c => c.ConversationId == id)
                .Include(c => c.ClientContact)
                .Include(c => c.AssignedByUser)
                .Include(u => u.AssignedAgent)
                .Include(u => u.Messages)
                    .ThenInclude(a=> a.Attachments)
                    .FirstOrDefaultAsync(ct);
            return conversation ?? throw new KeyNotFoundException($"Conversation with ID {id} not found.");
        }
    }
}