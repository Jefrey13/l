using CustomerService.API.Models;
namespace CustomerService.API.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellation = default);

        /// <summary>
        /// Limpia el campo LastOnline directamente en la base de datos.
        /// </summary>
        Task ClearLastOnlineAsync(int userId, CancellationToken cancellation = default);
    }
}