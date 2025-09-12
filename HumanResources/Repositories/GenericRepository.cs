using HumanResources.Data;
using HumanResources.Models; // Ensure this using is present for Client
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

            public virtual async Task<T> AddAsync(T entity)
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

    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        public ClientRepository(Context context) : base(context)
        {
        }

        public override async Task<Client> AddAsync(Client entity)
        {
            var created = await _context.Set<Client>()
                .FromSqlInterpolated(
                    $"EXEC dbo.spClient_Create {entity.CompanyName}, {entity.Nif}, {entity.Email}")
                .AsNoTracking()
                .SingleAsync(); // deve vir 1 linha
            return created;
        }

        public async Task<IEnumerable<Client>> GetClientsWithPendingTasksAsync()
        {
            // CS1061 fix: 'Client' does not contain a definition for 'Tasks'.
            // You must either:
            // 1. Add a 'Tasks' navigation property to the 'Client' class (if it exists in your model).
            // 2. Or, if 'Tasks' are not related to 'Client', remove or replace this query.

            // Option 1: If 'Tasks' should be a navigation property, add this to your Client class:
            // public ICollection<Task> Tasks { get; set; }

            // Option 2: If you do not have a 'Tasks' property, you need to clarify how to get pending tasks for clients.
            // For now, comment out the code to avoid the error:
            throw new NotImplementedException("Client does not have a 'Tasks' property. Please add it to the Client model or update this method.");
        }

        // Implement missing IGenericRepository<Client> members
        public async Task<List<Client>> GetAllAsync()
            => await base.GetAllAsync();

        public async Task<Client?> GetByIdAsync(int id)
            => await base.GetByIdAsync(id);

        public async Task UpdateAsync(Client entity)
            => await base.UpdateAsync(entity);

        public async Task DeleteAsync(int id)
            => await base.DeleteAsync(id);

        public async Task<bool> ExistsAsync(Expression<Func<Client, bool>> predicate)
            => await base.ExistsAsync(predicate);
    }
}
