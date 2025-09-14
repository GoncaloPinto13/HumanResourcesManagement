using HumanResources.Data;                         // DbContext da aplicação
using HumanResources.Models; // Ensure this using is present for Client // Necessário para referenciar a entidade Client
using Microsoft.EntityFrameworkCore;               // EF Core (DbSet, AsNoTracking, AnyAsync, etc.)
using System.Linq.Expressions;                     // Expression<Func<...>> para predicados

namespace HumanResources.Repositories
{
    /// Implementação base: concentra o acesso ao DbSet<T>.
    /// Mantém SaveChanges dentro do repositório para simplificar (sem Unit of Work).
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly Context _context;   // Instância do DbContext injetada
        protected readonly DbSet<T> _db;       // DbSet para o tipo T (tabela/entidade alvo)

        public GenericRepository(Context context)
        {
            _context = context;                // Guarda o contexto
            _db = _context.Set<T>();           // Obtém o DbSet<T> correspondente
        }

        public async Task<List<T>> GetAllAsync()
            => await _db.AsNoTracking().ToListAsync(); // Leitura sem tracking → melhor performance em cenários read-only

        public async Task<T?> GetByIdAsync(int id)
            => await _db.FindAsync(id);                 // Busca por PK; usa o cache do contexto se já estiver tracked

        public virtual async Task<T> AddAsync(T entity)
        {
            await _db.AddAsync(entity);                 // Marca entidade como Added
            await _context.SaveChangesAsync();          // Persiste imediatamente (design sem Unit of Work)
            return entity;                               // PK deverá estar preenchida após SaveChanges
        }

        public async Task UpdateAsync(T entity)
        {
            _db.Update(entity);                          // Marca como Modified (requer PK correta)
            await _context.SaveChangesAsync();           // Persiste alterações
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.FindAsync(id);        // Carrega a entidade por PK
            if (entity is null) return;                  // Se não existir, não faz nada
            _db.Remove(entity);                          // Marca para remoção
            await _context.SaveChangesAsync();           // Executa DELETE
        }

        public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
            => await _db.AnyAsync(predicate);            // Verifica existência com base no predicado
    }

    // Repositório concreto para Client, estende a implementação genérica e a interface IClientRepository
    
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        public ClientRepository(Context context) : base(context)
        {
        }

        public override async Task<Client> AddAsync(Client entity)
        {
            // Inserção via stored procedure.
            // FromSqlInterpolated usa parâmetros → evita SQL injection.
            
            var created = await _context.Set<Client>()
                .FromSqlInterpolated(
                    $"EXEC dbo.spClient_Create {entity.CompanyName}, {entity.Nif}, {entity.Email}")
                .AsNoTracking()               // Não trackeia o resultado; útil para evitar conflito de tracking
                .SingleAsync();               // Espera exatamente 1 linha; lança se 0 ou >1
            return created;
        }

        public async Task<IEnumerable<Client>> GetClientsWithPendingTasksAsync()
        {
            // Método placeholder: a entidade Client não tem navegação 'Tasks' definida.
            // Lança exceção explícita a indicar o que falta no modelo ou que o método deve ser reescrito.
            throw new NotImplementedException("Client does not have a 'Tasks' property. Please add it to the Client model or update this method.");
        }

        
        
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
