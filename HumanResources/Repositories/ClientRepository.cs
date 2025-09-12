using HumanResources.Data.Repositories;
using HumanResources.Models;
using HumanResources.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HumanResources.Data.Repositories
{
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

        public override async Task<Client> AddAsync(Client entity)
        {
            if (await ExistsAsync(c => c.Nif == entity.Nif))
                throw new Exception($"Já existe um cliente com NIF {entity.Nif}");

            return await base.AddAsync(entity);
        }


    }
}
