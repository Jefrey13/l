using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Implementations;
using CustomerService.API.Repositories.Interfaces;

namespace CustomerService.API.Repositories.Implementations
{
    public class CustomerDirectoryRepository: GenericRepository<CustomerDirectory>, ICustomerDirectoryRepository
    {
        public CustomerDirectoryRepository(CustomerSupportContext context)
            : base(context) { }
    }
}