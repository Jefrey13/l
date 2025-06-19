using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;

namespace CustomerService.API.Repositories.Implementations
{
    public class OpeningHourRepository: GenericRepository<OpeningHour>, IOpeningHourRepository
    {
        public OpeningHourRepository(CustomerSupportContext context) : base(context) { }
    }
}
