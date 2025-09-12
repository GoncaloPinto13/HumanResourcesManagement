using HumanResources.Data;
using HumanResources.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanResources.Repositories
{/// Repositório de Client: usa a base e acrescenta queries típicas.
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        public ClientRepository(Context context) : base(context) { }

        public async Task<Client?> GetByNifAsync(string nif)
            => await _context.Set<Client>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Nif == nif);

        public async Task<List<Client>> SearchByCompanyAsync(string prefix)
            => await _context.Set<Client>()
                .AsNoTracking()
                .Where(c => c.CompanyName.StartsWith(prefix))
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
    }
}
