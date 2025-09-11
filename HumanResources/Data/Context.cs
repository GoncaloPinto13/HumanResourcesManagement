using HumanResources.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // USANDO NECESSÁRIO
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HumanResources.Data
{
    // A MUDANÇA PRINCIPAL: Herdar de IdentityDbContext<User> em vez de DbContext
    public class Context : IdentityDbContext<User>
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        // Os seus DbSets para as entidades da aplicação
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Employee> Employees { get; set; }
        // Nota: Não é preciso adicionar DbSet<User>, o IdentityDbContext já trata disso.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // MUITO IMPORTANTE: Chamar a implementação base primeiro
            // para que as tabelas do Identity sejam configuradas.
            base.OnModelCreating(modelBuilder);

            // A configuração da relação N-N entre Project e Employee é agora automática,
            // porque as classes já têm as coleções uma da outra. O Entity Framework
            // irá criar a tabela de junção por si.

            // Este código continua a ser uma boa prática para evitar erros de eliminação em cascata.
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}

