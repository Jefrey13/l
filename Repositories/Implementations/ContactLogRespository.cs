using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class ContactLogRespository: GenericRepository<ContactLog>, IContactLogRepository
    {
        public ContactLogRespository(CustomerSupportContext context) : base(context) { }

        public async Task<ContactLog?> GetByPhone(string phoneNumber, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("El phoneNumber no puede ser vacío.", nameof(phoneNumber));

            return await _dbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(cl => cl.Phone == phoneNumber, cancellation);
        }
    }
}
