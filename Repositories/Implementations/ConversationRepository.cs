using CustomerService.API.Data.Context;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Utils;
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

        //Obtener cliente con una conversación en estado waiting o human y con mas de 1 minutos desde su ultimo mensaje.
       // Importante para listar a los usuarios con este periodo de tiempo
        public async Task<IEnumerable<WaitingClientResponseDto>> GetWaitingClient(FilterDashboard filters, CancellationToken ct = default)
        {

            var now = DateTime.Now;

            var intermediate = _dbSet
                .AsNoTracking()
                .Where(c =>
                    c.ClientContactId == filters.CustomerId
                    && (c.Status == ConversationStatus.Waiting || c.Status == ConversationStatus.Human)
                    && EF.Functions.DateDiffMinute(c.ClientLastMessageAt, now) > 1
                )
                .Select(c => new
                {
                    Id = c.ClientContactId,
                    Name = c.ClientContact.FullName,
                    AverageTime = EF.Functions.DateDiffMinute(c.ClientLastMessageAt, now)
                });


            var query = intermediate
                .GroupBy(x => new { x.Id, x.Name })
                .Select(g => new WaitingClientResponseDto
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    AverageTime = g.Average(x => x.AverageTime)
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

        public IQueryable<Conversation> QueryByState(string status)
        {
            if (!Enum.TryParse<ConversationStatus>(
                    status,
                    ignoreCase: true,
                    out var parsedStatus))
            {
                throw new ArgumentException(
                    $"Estado inválido: {status}",
                    nameof(status));
            }

            return _dbSet
                .AsNoTracking()
                .Where(c => c.Status == parsedStatus)
                .Include(c => c.ClientContact)
                .Include(c => c.AssignedAgent)
                .Include(c => c.AssignedByUser)
                .Include(c => c.Messages)
                .OrderBy(c => c.CreatedAt);
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

        public async Task<ResponseAgentAverageResponseDto> ResponseAgentAverageAsync(
    FilterDashboard filters,
    CancellationToken ct = default)
        {
            try
            {
                var agentId = filters.AgentId;

                //  Obtener el nombre del agente
                var agentName = await _dbSet.AsNoTracking()
                    .Where(u => u.AssignedAgentId == agentId)
                    .Select(u => u.AssignedAgent.FullName)
                    .FirstOrDefaultAsync(ct);

                // Explode de mensajes de cliente y buscar la respuesta del agente
                var clientAgentPairs = _dbSet
                    .AsNoTracking()
                    .Where(conv =>
                        conv.AssignedAgentId == agentId &&
                        conv.CreatedAt.Date >= filters.From.Value.Date &&
                        conv.CreatedAt.Date <= filters.To.Value.Date)
                    .SelectMany(conv => conv.Messages
                        .Where(m => m.SenderContactId != null)
                        .Select(m => new {
                            ConvId = conv.ConversationId,
                            ClientId = m.SenderContactId.Value,
                            ClientName = conv.ClientContact.FullName,
                            ClientAt = m.DeliveredAt
                        })
                    )
                    .Select(x => new {
                        x.ClientId,
                        x.ClientName,
                        // Primer mensaje del agente posterior a ClientAt
                        AgentAt = _context.Messages
                            .Where(m2 =>
                                m2.ConversationId == x.ConvId &&
                                m2.Conversation.AssignedAgentId == agentId &&
                                m2.DeliveredAt > x.ClientAt)
                            .OrderBy(m2 => m2.DeliveredAt)
                            .Select(m2 => (DateTimeOffset?)m2.DeliveredAt)
                            .FirstOrDefault(),
                        x.ConvId,
                        x.ClientAt
                    })
                    .Where(x => x.AgentAt.HasValue)
                    .Select(x => new {
                        x.ClientId,
                        x.ClientName,
                        ResponseSeconds = EF.Functions.DateDiffSecond(x.ClientAt, x.AgentAt.Value),
                        x.ConvId
                    });

                // Agrupar por cliente, calcular promedio y cuenta de conversaciones
                var topClients = await clientAgentPairs
                    .GroupBy(x => new { x.ClientId, x.ClientName })
                    .Select(g => new ResponseAgentAverageClienteDataResponseDto
                    {
                        ClienteId = g.Key.ClientId,
                        ClientName = g.Key.ClientName,
                        AverageSeconds = g.Average(x => x.ResponseSeconds),
                        ConversationCount = g.Select(x => x.ConvId).Distinct().Count()
                    })
                    .OrderByDescending(dto => dto.ConversationCount)
                    .Take(10)
                    .ToListAsync(ct);

                var result = new ResponseAgentAverageResponseDto
                {
                    Id = (int)agentId,
                    AgentName = agentName,
                    ClienteData = topClients
                };

                return result;
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ResponseAgentAverageResponseDto();
            }
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