using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Utils.Enums;
using Microsoft.EntityFrameworkCore;
using WhatsappBusiness.CloudApi.Webhook;

namespace CustomerService.API.Repositories.Implementations
{
    public class WorkShiftRepository: GenericRepository<WorkShift_User>, IWorkShiftRepository
    {
        public WorkShiftRepository(CustomerSupportContext context) : base(context) { }

        public override IQueryable<WorkShift_User> GetAll()
            => _dbSet
                .Include(c => c.AssignedUser)
                .Include(c => c.CreatedBy)
                .Include(c => c.UpdatedBy)
                .Include(c => c.OpeningHour);


        public override async Task<WorkShift_User> GetByIdAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0) throw new ArgumentException("El id debe ser mayor que cero", nameof(id));

            var entity = await _dbSet
                .Where(ws => ws.Id == id)
                .Include(c => c.AssignedUser)
                .Include(c => c.CreatedBy)
                .Include(c => c.UpdatedBy)
                .Include(c => c.OpeningHour)
                .FirstOrDefaultAsync(ct);

            return entity ?? throw new KeyNotFoundException($"WorkShift with ID {id} not found.");
        }

    }
}