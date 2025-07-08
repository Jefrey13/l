using CustomerService.API.Data.Context;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Utils.Enums;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                .Where(c => c.AssignedAgentId == agentId &&( c.AssignmentState == AssignmentState.Forced ||
                c.AssignmentState == AssignmentState.Accepted) )
                .Include(c => c.ClientContact)
                .Include(c => c.AssignedAgent)
                .Include(c => c.AssignedByUser)
                .Include(c => c.Messages)
                    .ThenInclude(a=> a.Attachments)
                .ToListAsync(cancellation);
        }

        public async Task<IEnumerable<ConversationStatusCountResponseDto>> GetConversationsCountByDateRange
            (DateTime from, DateTime to, CancellationToken ct = default)
        {
            if (from > to)
                throw new ArgumentException("La fecha 'from' debe ser anterior o igual a 'to'.");

            var query = _dbSet
              .AsNoTracking()
              .Where(c => c.CreatedAt.Date >= from.Date && c.CreatedAt.Date <= to.Date.AddDays(1))
              .GroupBy(c => c.Status)
              .Select(g => new ConversationStatusCountResponseDto
              {
                  Status = g.Key.HasValue
                             ? g.Key.Value.ToString()
                             : "Unknown",
                  Count = g.Count()
              });

            return await query.ToListAsync(ct);

        }

        public async Task<IEnumerable<AdminAsigmentResponseTimeResponseDto>> AssigmentResponseTimeAsync
            (DateTime from, DateTime to, CancellationToken ct = default)
        {

            //Tiempo promedio de asignación de agente por parte del administradores.
            var intermediate = _dbSet
                .AsNoTracking()
                .Where(c => c.AgentRequestAt != null
                && c.AssignedAt != null)
                .Select(c => new
                {
                    Id = c.AssignedByUserId,
                    Name = c.AssignedByUser.FullName,
                    Seconds = EF.Functions.DateDiffSecond(
                        c.AgentRequestAt.Value,
                        c.AssignedAt
                        )
                });

            var query = intermediate.
                GroupBy(x => new { x.Id, x.Name })
                .Select(g => new AdminAsigmentResponseTimeResponseDto
                {
                   Id = g.Key.Id,
                   Name = g.Key.Name!,
                   averageTime = g.Average(x => x.Seconds)
                });

            return await query.ToListAsync(ct);
        }
        public async Task<IEnumerable<AverageAssignmentTimeResponseDto>>AverageAssignmentTimeAsync(DateTime from, DateTime to, CancellationToken ct = default)
        {
            var intermediate = _dbSet
                .AsNoTracking()
                .Where(c => (c.AssignedAgentId != null
                         && c.AgentRequestAt != null
                         && c.AssignedAt != null)
                         && (c.CreatedAt.Date >= from.Date && c.CreatedAt.Date <= to.Date)
                         )
                .Select(c => new
                {
                    Id = c.AssignedAgentId.Value,
                    Name = c.AssignedAgent.FullName,
                    averageTime = EF.Functions.DateDiffSecond(
                                    c.AgentRequestAt.Value,
                                    c.AssignedAt.Value
                                )
                });

            var query = intermediate
                .GroupBy(x => new { x.Id, x.Name })
                .Select(g => new AverageAssignmentTimeResponseDto
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    averageTime = g.Average(x => x.averageTime)
                });

            return await query.ToListAsync(ct);
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
            //.Where(c=> c.Status != ConversationStatus.Closed)
                .Include(c => c.ClientContact)
                .Include(c => c.AssignedAgent)
                .Include(c => c.Messages)
                        .ThenInclude(a=>  a.Attachments);

        //public async Task<Conversation> GetByStatusAsync(List<ConversationStatus> statuses, CancellationToken cancellation = default)
        //{
        //    var conv = await _dbSet.Where(c=> c.Status 
        //    != ConversationStatus.Closed || c.Status != ConversationStatus.Incomplete).ToListAsync();

        //    return conv.FirstOrDefault();
        //}

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