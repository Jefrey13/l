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

        //Obtener las conversaciones por compañia(Todos los contactos que con de un cliente).
        public  IQueryable<Conversation> GetConversationByClient(FilterDashboard filters,
            CancellationToken ct = default)
        {
            var convs = _dbSet.AsNoTracking()
                .Where(c => c.CreatedAt.Date >= filters.From!.Value.Date && c.CreatedAt.Date <= filters.To!.Value.Date)
                .Include(c => c.ClientContact)
                .Include(c => c.AssignedByUser)
                .Include(u => u.AssignedAgent)
                .Include(u => u.Messages)
                    .ThenInclude(a => a.Attachments)
                .OrderByDescending(c => c.CreatedAt);

            return convs;
        }

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

            var timeParam = _context.SystemParams
            .FirstOrDefault(e => e.Name == "ClientCriticalStateTime");


            var intermediate = _dbSet
                .AsNoTracking()
                .Where(c =>
                    //c.ClientContactId == filters.CustomerId
                     (c.Status == ConversationStatus.Waiting || c.Status == ConversationStatus.Human)
                    && EF.Functions.DateDiffMinute(c.ClientLastMessageAt, now) > int.Parse(timeParam!.Value)
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
                // Validaciones básicas
                if (!filters.AgentId.HasValue)
                    throw new ArgumentException("El campo 'AgentId' no puede ser null.");

                if (!filters.From.HasValue || !filters.To.HasValue)
                    throw new ArgumentException("Debe proporcionar fechas válidas en 'From' y 'To'.");

                var agentId = filters.AgentId.Value;

                // Obtener nombre del agente (puede ser null si no hay datos)
                var agentName = await _dbSet
                    .AsNoTracking()
                    .Where(c => c.AssignedAgentId == agentId && c.AssignedAgent != null)
                    .Select(c => c.AssignedAgent!.FullName)
                    .FirstOrDefaultAsync(ct);

                //  Obtener todos los mensajes de clientes en conversaciones asignadas al agente
                //Ojo si es senderuserid es 1 es por que lo envio el bot y no el agente. Y si es null, en que envio el nensaje fue el cliente
                var rawMessages = await _dbSet
                    .AsNoTracking()
                    .Where(conv =>
                        conv.AssignedAgentId == agentId &&
                        conv.CreatedAt.Date >= filters.From.Value.Date &&
                        conv.CreatedAt.Date <= filters.To.Value.Date)
                    .SelectMany(conv => conv.Messages
                        .Where(m => m.SenderContactId != null)
                        .Select(m => new
                        {
                            ConvId = conv.ConversationId,
                            ClientId = m.SenderContactId!.Value,
                            ClientName = conv.ClientContact.FullName,
                            ClientAt = m.SentAt
                        }))
                    .ToListAsync(ct);

                // Otener todos los mensajes del agente para esas conversaciones
                var agentMessages = await _context.Messages
                    .AsNoTracking()
                    .Where(m =>
                        m.Conversation.AssignedAgentId == agentId &&
                        m.SentAt != null &&
                        m.SenderUserId != null && m.SenderUserId == filters.AgentId
                        )
                    .OrderBy(m => m.SentAt)
                    .Select(m => new
                    {
                        m.ConversationId,
                        m.SentAt
                    })
                    .ToListAsync(ct);

                // Calcular tiempo de respuesta por cada mensaje de cliente
                var responseData = rawMessages
                    .Select(m =>
                    {
                        var firstResponse = agentMessages
                            .Where(am => am.ConversationId == m.ConvId && am.SentAt > m.ClientAt)
                            .FirstOrDefault();

                        return firstResponse != null
                            ? new
                            {
                                m.ClientId,
                                m.ClientName,
                                ConvId = m.ConvId,
                                ResponseSeconds = (int)(firstResponse.SentAt - m.ClientAt).TotalSeconds
                            }
                            : null;
                    })
                    .Where(x => x != null)
                    .ToList()!;

                //  Agrupar por cliente y calcular promedio y número de conversaciones
                var topClients = responseData
                    .GroupBy(x => new { x.ClientId, x.ClientName })
                    .Select(g => new ResponseAgentAverageClienteDataResponseDto
                    {
                        ClienteId = g.Key.ClientId,
                        ClientName = g.Key.ClientName,
                        AverageSeconds = g.Average(x => x!.ResponseSeconds),
                        ConversationCount = g.Select(x => x!.ConvId).Distinct().Count()
                    })
                    .OrderByDescending(dto => dto.ConversationCount)
                    .Take(10)
                    .ToList();

                // Armar el DTO final
                var result = new ResponseAgentAverageResponseDto
                {
                    Id = agentId,
                    AgentName = agentName ?? "No disponible",
                    ClienteData = topClients
                };

                return result;
            }
            catch (Exception ex)
            {
                // Aquí puedes registrar el error usando ILogger si tienes inyectado el logger
                Console.WriteLine($"[ERROR] {ex.Message}");

                // Devuelve un objeto vacío pero válido
                return new ResponseAgentAverageResponseDto
                {
                    Id = filters.AgentId ?? 0,
                    AgentName = "Error",
                    ClienteData = new List<ResponseAgentAverageClienteDataResponseDto>()
                };
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