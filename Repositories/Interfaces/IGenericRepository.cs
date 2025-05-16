using System.Linq.Expressions;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetAll();
    Task<T?> GetByIdAsync(int id, CancellationToken cancellation = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellation = default);
    Task AddAsync(T entity, CancellationToken cancellation = default);
    void Update(T entity, CancellationToken cancellation = default);
    void Remove(T entity, CancellationToken cancellation = default);
}
