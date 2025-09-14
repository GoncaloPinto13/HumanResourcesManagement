using HumanResources.Data.Repositories; // redundante, o namespace abaixo já é o mesmo; não causa erro, mas é dispensável
using HumanResources.Models;
using HumanResources.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HumanResources.Data.Repositories
{
    // Repositório específico de Client, baseado num repositório genérico e interface IClientRepository
    // Encapsula queries comuns (por NIF e por prefixo do nome da empresa).
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        public ClientRepository(Context context) : base(context) { }

        // Obtém um Client por NIF (ou null se não existir).
        // AsNoTracking: leitura sem tracking → melhor performance para read-only.
        
        public async Task<Client?> GetByNifAsync(string nif)
            => await _context.Set<Client>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Nif == nif);

        // Pesquisa por nome da empresa começando pelo prefixo
        // AsNoTracking: leitura; OrderBy para resultados estáveis
        
        public async Task<List<Client>> SearchByCompanyAsync(string prefix)
            => await _context.Set<Client>()
                .AsNoTracking()
                .Where(c => c.CompanyName.StartsWith(prefix))
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

        // Override de AddAsync para impedir inserção com NIF duplicado
        // Verifica existência e lança Exception se já existir; depois delega no repositório base
        
        public override async Task<Client> AddAsync(Client entity)
        {
            if (await ExistsAsync(c => c.Nif == entity.Nif))
                throw new Exception($"Já existe um cliente com NIF {entity.Nif}");

            return await base.AddAsync(entity);
        }


    }
}
