using HumanResources.Models; // Necessário para referenciar a entidade Client

namespace HumanResources.Repositories
{

    /// Interface do repositório específico de Client.
    /// Herda operações CRUD de IGenericRepository<Client> e adiciona queries típicas de leitura
    public interface IClientRepository : IGenericRepository<Client>
    {
        Task<Client?> GetByNifAsync(string nif);                 // Obtém um cliente por NIF; retorna null se não existir. Assume NIF único
        Task<List<Client>> SearchByCompanyAsync(string prefix);  // Pesquisa por prefixo do nome da empresa. Depende da collation para case-sensitivity
    }
}
