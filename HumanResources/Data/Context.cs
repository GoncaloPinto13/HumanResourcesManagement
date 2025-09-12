using HumanResources.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanResources.Data
{
    public class Context : DbContext
    {
        public Context (DbContextOptions<Context> options) : base(options)
        {
        }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Contract> Contracts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- SOLUÇÃO DEFINITIVA PARA O ERRO DE "CYCLES OR MULTIPLE CASCADE PATHS" ---
            // Este código desativa a eliminação em cascata para TODAS as relações,
            // impedindo que o SQL Server encontre caminhos ambíguos para apagar dados.
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // --- Configuração explícita das relações (Boas práticas) ---

            // Configura a relação 1-para-1 entre Project e Contract
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Contract)
                .WithOne(c => c.Project)
                .HasForeignKey<Contract>(c => c.ProjectId);

            // Configura a relação N-N entre Project e Employee, especificando a tabela de junção
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Employees)
                .WithMany(e => e.Projects)
                .UsingEntity(j => j.ToTable("ProjectEmployees"));

            modelBuilder.Entity<Client>(e =>
            {
                e.Property(p => p.CompanyName).HasMaxLength(150).IsRequired();
                e.Property(p => p.Nif).HasMaxLength(9).IsRequired();
                e.Property(p => p.Email).HasMaxLength(150);

                e.HasIndex(p => p.Nif).IsUnique(); // NIF único
                e.HasIndex(p => p.Email);          // índice normal
            });

        }
    }
}
