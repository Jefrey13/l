using CustomerService.API.Data.Context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;

namespace CustomerService.API.Repositories.Implementations
{
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        public TagRepository(CustomerSupportContext context)
            : base(context)
        {
        }
    }
}
