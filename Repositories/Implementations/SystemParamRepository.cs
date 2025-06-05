using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class SystemParamRepository: GenericRepository<SystemParam>, ISystemParamRepository
    {

        public SystemParamRepository(CustomerSupportContext context) : base(context)
        {
        }

        public async Task<SystemParam?> GetByNameAsync(string name, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre no puede ser nulo o vacío.", nameof(name));

            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(sp => sp.Name == name, cancellation);
        }
    }
}