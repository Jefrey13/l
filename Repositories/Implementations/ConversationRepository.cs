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

        public async Task<IEnumerable<ConversationStatusCountResponseDto>> GetConversationsCountByDateRange(DateTime from, DateTime to, CancellationToken ct = default)
        {
            // Valida fechas (opcional)
            if (from > to)
                throw new ArgumentException("La fecha 'from' debe ser anterior o igual a 'to'.");

            var query = _dbSet
              .AsNoTracking()
              .Where(c => c.CreatedAt >= from && c.CreatedAt <= to)
              .GroupBy(c => c.Status)
              .Select(g => new ConversationStatusCountResponseDto
              {
                  Status = g.Key.HasValue
                             ? g.Key.Value.ToString()
                             : "Unknown",    // o el texto que prefieras para nulos
                  Count = g.Count()
              });

            return await query.ToListAsync(ct);

        }

        public async Task<IEnumerable<AverageAssignmentTimeResponseDto>>
     AverageAssignmentTimeAsync(CancellationToken ct = default)
        {
            // 1) Proyección inicial: saco Id, Nombre y segundos de cada asignación
            var intermediate = _dbSet
                .AsNoTracking()
                .Where(c => c.AssignedAgentId != null
                         && c.AgentRequestAt != null
                         && c.AssignedAt != null)
                .Select(c => new
                {
                    AgentId = c.AssignedAgentId.Value,
                    AgentName = c.AssignedAgent.FullName,   // navega a la propiedad
                    Seconds = EF.Functions.DateDiffSecond(
                                    c.AgentRequestAt.Value,
                                    c.AssignedAt.Value
                                )
                });

            // 2) Agrupo por AgentId + AgentName y calculo promedio de Seconds
            var query = intermediate
                .GroupBy(x => new { x.AgentId, x.AgentName })
                .Select(g => new AverageAssignmentTimeResponseDto
                {
                    AgentId = g.Key.AgentId,
                    AgentName = g.Key.AgentName,
                    AverageSeconds = g.Average(x => x.Seconds)
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