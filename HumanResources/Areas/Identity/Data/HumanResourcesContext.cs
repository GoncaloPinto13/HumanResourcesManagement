using HumanResources.Areas.Identity.Data;
using HumanResources.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HumanResources.Data;

public class HumanResourcesContext : IdentityDbContext<HumanResourcesUser>
{
    public HumanResourcesContext(DbContextOptions<HumanResourcesContext> options)
        : base(options)
    {
    }

    //protected override void OnModelCreating(ModelBuilder builder)
    //{
    //    base.OnModelCreating(builder);
    //    // Customize the ASP.NET Identity model and override the defaults if needed.
    //    // For example, you can rename the ASP.NET Identity table names and more.
    //    // Add your customizations after calling base.OnModelCreating(builder);
    //}


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
    }

}
