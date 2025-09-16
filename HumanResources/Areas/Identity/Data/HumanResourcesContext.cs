using HumanResources.Areas.Identity.Data;
using HumanResources.Models;
using HumanResources.ViewModels; // ADICIONADO: Para o ViewModel do relatório
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HumanResources.Data
{
    public class HumanResourcesContext : IdentityDbContext<HumanResourcesUser>
    {
        public HumanResourcesContext(DbContextOptions<HumanResourcesContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Contract> Contracts { get; set; }

        // --- NOME CORRIGIDO PARA O PLURAL ---
        public DbSet<EmployeeContract> EmployeeContracts { get; set; }

        public DbSet<ProjectPerformanceViewModel> ProjectPerformanceReport { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // É crucial chamar a implementação base primeiro para configurar o Identity
            base.OnModelCreating(modelBuilder);

            // --- Configuração para o ViewModel do Relatório ---
            // Diz ao EF Core que este tipo não tem uma tabela nem chave primária
            modelBuilder.Entity<ProjectPerformanceViewModel>().HasNoKey();


            // --- Configurações das Relações de Negócio (Opcional, mas boa prática) ---
            
            // Relação N-N entre Project e Employee é gerida implicitamente pelo EF Core
            // porque ambos os modelos têm uma ICollection um do outro.
            // A configuração explícita foi removida para evitar conflitos.

            // Relação 1-N: Um Projeto tem muitos Contratos
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Project)
                .WithMany(p => p.Contracts)
                .HasForeignKey(c => c.ProjectId);

            // Relação 1-N: Um Funcionário tem muitas associações a contratos (EmployeeContracts)
            modelBuilder.Entity<EmployeeContract>()
                .HasOne(ec => ec.Employee)
                .WithMany(e => e.EmployeeContracts)
                .HasForeignKey(ec => ec.EmployeeId);

            // Relação 1-N: Um Contrato tem muitas associações a funcionários (EmployeeContracts)
            modelBuilder.Entity<EmployeeContract>()
                .HasOne(ec => ec.Contract)
                .WithMany(c => c.EmployeeContracts)
                .HasForeignKey(ec => ec.ContractId);


            // --- Prevenção de Eliminação em Cascata Múltipla ---
            // Boa prática para evitar erros no SQL Server
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

    }

}





