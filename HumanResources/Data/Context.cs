//using HumanResources.Models;
//using Microsoft.EntityFrameworkCore;

//namespace HumanResources.Data
//{
//    public class Context : DbContext
//    {
//        public Context(DbContextOptions<Context> options) : base(options)
//        {

//        }
//        public DbSet<Client> Clients { get; set; }
//        public DbSet<Employee> Employees { get; set; }
//        public DbSet<Project> Projects { get; set; }
//        public DbSet<Contract> Contracts { get; set; }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            // --- SOLUÇÃO DEFINITIVA PARA O ERRO DE "CYCLES OR MULTIPLE CASCADE PATHS" ---
//            // Este código desativa a eliminação em cascata para TODAS as relações,
//            // impedindo que o SQL Server encontre caminhos ambíguos para apagar dados.
//            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
//            {
//                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
//            }

//            /// Configura a relação 1-para-1 entre Project e Contract
//            modelBuilder.Entity<Project>()
//                .HasOne(p => p.Contract)
//                .WithOne(c => c.Project)
//                .HasForeignKey<Contract>(c => c.ProjectId);

//            // --- Configuração das NOVAS relações Muitos-para-Muitos explícitas ---

//            // Relação de 1-para-Muitos: Um Funcionário tem muitos EmployeeContracts
//            modelBuilder.Entity<EmployeeContract>()
//                .HasOne(ec => ec.Employee)
//                .WithMany(e => e.EmployeeContracts)
//                .HasForeignKey(ec => ec.EmployeeId);

//            // Relação de 1-para-Muitos: Um Contrato tem muitos EmployeeContracts
//            modelBuilder.Entity<EmployeeContract>()
//                .HasOne(ec => ec.Contract)
//                .WithMany(c => c.EmployeeContracts)
//                .HasForeignKey(ec => ec.ContractId);

//        }
//    }
//}
using HumanResources.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanResources.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Contract> Contracts { get; set; }

        // --- Adicione esta linha que faltava no seu ficheiro ---
        public DbSet<EmployeeContract> EmployeeContracts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Desativa a eliminação em cascata para TODAS as relações
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Configura a relação 1-para-1 entre Project e Contract
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Contract)
                .WithOne(c => c.Project)
                .HasForeignKey<Contract>(c => c.ProjectId);

            // Configura a relação 1-para-Muitos: Um Funcionário tem muitos EmployeeContracts
            modelBuilder.Entity<EmployeeContract>()
                .HasOne(ec => ec.Employee)
                .WithMany(e => e.EmployeeContracts)
                .HasForeignKey(ec => ec.EmployeeId);

            // Configura a relação 1-para-Muitos: Um Contrato tem muitos EmployeeContracts
            modelBuilder.Entity<EmployeeContract>()
                .HasOne(ec => ec.Contract)
                .WithMany(c => c.EmployeeContracts)
                .HasForeignKey(ec => ec.ContractId);
        }
    }
}