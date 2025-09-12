using System.Linq.Expressions;

namespace HumanResources.Repositories
{
    // Change from class to interface and make it generic
    public interface IGenericRepository<T>
    {
        Task<List<T>> GetAllAsync();                         // SELECT * (sem tracking)
        Task<T?> GetByIdAsync(int id);                      // SELECT por PK
        Task<T> AddAsync(T entity);                         // INSERT + SaveChanges
        Task UpdateAsync(T entity);                         // UPDATE + SaveChanges
        Task DeleteAsync(int id);                           // DELETE por PK + SaveChanges
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate); // EXISTS
    }
}
