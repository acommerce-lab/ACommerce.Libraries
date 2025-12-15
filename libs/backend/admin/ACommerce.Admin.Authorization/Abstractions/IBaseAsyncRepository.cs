using System.Linq.Expressions;

namespace ACommerce.Admin.Authorization.Abstractions;

public interface IBaseAsyncRepository<T> where T : class, IBaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
