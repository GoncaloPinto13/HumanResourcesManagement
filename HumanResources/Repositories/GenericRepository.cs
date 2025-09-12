using HumanResources.Data;
using Microsoft.EntityFrameworkCore;

namespace HumanResources.Repositories
{
        /// Implementação base: concentra o acesso ao DbSet<T>.
        /// Mantém SaveChanges dentro do repositório para simplificar (sem Unit of Work).
        public class GenericRepository<T> : IGenericRepository<T> where T : class
        {
            protected readonly Context _context;
            protected readonly DbSet<T> _db;

            public GenericRepository(Context context)
            {
                _context = context;
                _db = _context.Set<T>();
            }

            public async Task<List<T>> GetAllAsync()
                => await _db.AsNoTracking().ToListAsync(); // leitura sem tracking = mais rápido

            public async Task<T?> GetByIdAsync(int id)
                => await _db.FindAsync(id);                 // usa cache do contexto se existir

            public async Task<T> AddAsync(T entity)
            {
                await _db.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;                               // já deverá ter PK preenchida
            }

            public async Task UpdateAsync(T entity)
            {
                _db.Update(entity);                          // exige PK correta
                await _context.SaveChangesAsync();
            }

            public async Task DeleteAsync(int id)
            {
                var entity = await _db.FindAsync(id);
                if (entity is null) return;                  // nada a apagar
                _db.Remove(entity);
                await _context.SaveChangesAsync();
            }

            public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
                => await _db.AnyAsync(predicate);

        }
}
