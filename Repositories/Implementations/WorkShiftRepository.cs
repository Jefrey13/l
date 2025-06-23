using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Utils.Enums;
using Microsoft.EntityFrameworkCore;
using WhatsappBusiness.CloudApi.Webhook;

namespace CustomerService.API.Repositories.Implementations
{
    public class WorkShiftRepository : GenericRepository<WorkShift_User>, IWorkShiftRepository
    {
        public WorkShiftRepository(CustomerSupportContext context) : base(context) { }

        public override IQueryable<WorkShift_User> GetAll() =>
            _dbSet
                .Include(ws => ws.AssignedUser)
                .Include(ws => ws.CreatedBy)
                .Include(ws => ws.UpdatedBy)
                .Include(ws => ws.OpeningHour);

        public override async Task<WorkShift_User> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _dbSet
                .Include(ws => ws.AssignedUser)
                .Include(ws => ws.CreatedBy)
                .Include(ws => ws.UpdatedBy)
                .Include(ws => ws.OpeningHour)
                .FirstOrDefaultAsync(ws => ws.Id == id, ct);
            if (entity == null) throw new KeyNotFoundException($"WorkShift_User with ID {id} not found.");
            return entity;
        }

        public async Task<int> GetActiveAssignmentsCountAsync(DateOnly date, CancellationToken ct = default) =>
            await _dbSet
                .AsNoTracking()
                .CountAsync(ws =>
                    ws.IsActive
                    && (ws.ValidFrom == null || ws.ValidFrom <= date)
                    && (ws.ValidTo == null || ws.ValidTo >= date),
                ct);

        public async Task<IEnumerable<WorkShift_User>> GetByDateAsync(DateOnly date, CancellationToken ct = default) =>
            await _dbSet
                .AsNoTracking()
                .Include(ws => ws.AssignedUser)
                .Include(ws => ws.OpeningHour)
                .Where(ws =>
                    ws.IsActive
                    && (ws.ValidFrom == null || ws.ValidFrom <= date)
                    && (ws.ValidTo == null || ws.ValidTo >= date))
                .ToListAsync(ct);
    }
}