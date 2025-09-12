using HumanResources.Models;

namespace HumanResources.Repositories
{
  
    /// Métodos específicos para Client além do CRUD.
    public interface IClientRepository : IGenericRepository<Client>
    {
        Task<Client?> GetByNifAsync(string nif);                 // busca 1 por NIF
        Task<List<Client>> SearchByCompanyAsync(string prefix);  // pesquisa por prefixo do nome
    }
}
