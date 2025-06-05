    using CustomerService.API.Models;

    namespace CustomerService.API.Repositories.Interfaces
    {
        public interface ISystemParamRepository: IGenericRepository<SystemParam>
        {
           Task<SystemParam?> GetByNameAsync(string name, CancellationToken cancellation = default);
        }
    }
