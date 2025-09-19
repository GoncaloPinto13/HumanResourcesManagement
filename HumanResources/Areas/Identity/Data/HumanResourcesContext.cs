// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
using HumanResources.Areas.Identity.Data;
using HumanResources.Models;
using HumanResources.ViewModels; // Importa o ViewModel usado para o relatório.
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

// O namespace agrupa as classes relacionadas com o acesso a dados.
namespace HumanResources.Data
{
    /// <summary>
    /// A classe de contexto da base de dados para a aplicação.
    /// Herda de 'IdentityDbContext<HumanResourcesUser>', o que significa que
    /// já vem pré-configurada para gerir todas as tabelas do ASP.NET Core Identity
    /// (Users, Roles, Claims, etc.), usando a nossa classe de utilizador personalizada.
    /// </summary>
    public class HumanResourcesContext : IdentityDbContext<HumanResourcesUser>
    {
        /// <summary>
        /// Construtor que recebe as opções de configuração do DbContext (como a connection string)
        /// e as passa para a classe base.
        /// </summary>
        public HumanResourcesContext(DbContextOptions<HumanResourcesContext> options)
            : base(options)
        {
        }

        // --- Mapeamento das Entidades para Tabelas (DbSets) ---
        // Cada propriedade DbSet<T> representa uma tabela na base de dados que pode ser consultada.
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Contract> Contracts { get; set; }

        // Tabela de junção para a relação Muitos-para-Muitos entre Employee e Contract.
        public DbSet<EmployeeContract> EmployeeContracts { get; set; }

        // Este DbSet é especial. Ele não representa uma tabela, mas sim o tipo de dados
        // que será retornado por uma consulta SQL bruta ou Stored Procedure.
        public DbSet<ProjectPerformanceViewModel> ProjectPerformanceReport { get; set; }


        /// <summary>
        /// Este método é chamado pelo Entity Framework Core quando o modelo da base de dados
        /// está a ser criado pela primeira vez. É aqui que se usam as "Fluent APIs"
        /// para configurar detalhadamente o modelo.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // É CRUCIAL chamar a implementação da classe base primeiro.
            // Isto garante que todo o mapeamento necessário para as tabelas do Identity é configurado.
            base.OnModelCreating(modelBuilder);

            // --- Configuração para o ViewModel do Relatório ---
            // Informa ao EF Core que o tipo 'ProjectPerformanceViewModel' não é uma entidade padrão.
            // Ele não tem uma chave primária e não deve ser mapeado para uma tabela.
            // Isto é necessário para que possamos usá-lo como o tipo de retorno de `FromSqlRaw`.
            modelBuilder.Entity<ProjectPerformanceViewModel>().HasNoKey();


            // --- Configurações das Relações de Negócio (Opcional, mas boa prática) ---
            // Embora o EF Core consiga inferir muitas relações por convenção,
            // configurá-las explicitamente torna o modelo mais claro e robusto.

            // Relação 1-N: Um Projeto (Project) tem muitos Contratos (Contracts).
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Project)          // Cada Contrato tem um Projeto.
                .WithMany(p => p.Contracts)      // Cada Projeto tem muitos Contratos.
                .HasForeignKey(c => c.ProjectId); // A chave estrangeira está em Contract.ProjectId.

            // Relação 1-N: Um Funcionário (Employee) tem muitas associações (EmployeeContracts).
            modelBuilder.Entity<EmployeeContract>()
                .HasOne(ec => ec.Employee)
                .WithMany(e => e.EmployeeContracts)
                .HasForeignKey(ec => ec.EmployeeId);

            // Relação 1-N: Um Contrato (Contract) tem muitas associações (EmployeeContracts).
            modelBuilder.Entity<EmployeeContract>()
                .HasOne(ec => ec.Contract)
                .WithMany(c => c.EmployeeContracts)
                .HasForeignKey(ec => ec.ContractId);


            // --- Prevenção de Eliminação em Cascata Múltipla ---
            // Esta é uma configuração de segurança e integridade muito importante.
            // O SQL Server não permite múltiplos caminhos de eliminação em cascata para a mesma tabela.
            // Este código percorre todas as chaves estrangeiras do modelo...
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                // ...e define o seu comportamento de eliminação para 'Restrict'.
                // Isto significa que o EF Core irá impedir a eliminação de um registo principal (ex: um Cliente)
                // se existirem registos dependentes (ex: Projetos associados a esse Cliente).
                // A eliminação dos registos dependentes terá de ser feita manualmente na aplicação.
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}