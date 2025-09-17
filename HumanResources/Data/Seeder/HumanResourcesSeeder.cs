using HumanResources.Areas.Identity.Data;
using HumanResources.Data;
using HumanResources.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.Data.Seeder
{
    public class HumanResourcesSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HumanResourcesContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<HumanResourcesUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<HumanResourcesSeeder>>();

                try
                {
                    logger.LogInformation("A aplicar migrações da base de dados...");
                    await context.Database.MigrateAsync();

                    await SeedRolesAndAdminAsync(userManager, roleManager, logger);

                    if (!context.Clients.Any())
                    {
                        logger.LogInformation("A popular a base de dados com dados de negócio...");
                        await SeedBusinessDataAsync(context, userManager, logger);
                    }
                    else
                    {
                        logger.LogInformation("A base de dados já contém dados de negócio. Seeding ignorado.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ocorreu um erro durante o seeding da base de dados.");
                }
            }
        }

        private static async Task SeedRolesAndAdminAsync(UserManager<HumanResourcesUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<HumanResourcesSeeder> logger)
        {
            string[] roleNames = { "Admin", "Project Manager", "Employee", "Cliente" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation($"Perfil '{roleName}' criado.");
                }
            }

            var adminEmail = "admin@hr.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new HumanResourcesUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(adminUser, "Admin@123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Utilizador Admin criado e associado ao perfil 'Admin'.");
                }
            }
        }

        private static async Task SeedBusinessDataAsync(HumanResourcesContext context, UserManager<HumanResourcesUser> userManager, ILogger<HumanResourcesSeeder> logger)
        {
            // --- 1. Criação de Clientes e Funcionários (e seus Users) ---
            var clientUser1 = new HumanResourcesUser { UserName = "cliente.tech@email.com", Email = "cliente.tech@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(clientUser1, "Cliente@123!");
            await userManager.AddToRoleAsync(clientUser1, "Cliente");
            var client1 = new Client { CompanyName = "Tech Solutions", Nif = "508123456", UserId = clientUser1.Id };

            var clientUser2 = new HumanResourcesUser { UserName = "cliente.innovate@email.com", Email = "cliente.innovate@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(clientUser2, "Cliente@123!");
            await userManager.AddToRoleAsync(clientUser2, "Cliente");
            var client2 = new Client { CompanyName = "Innovate Co.", Nif = "509123789", UserId = clientUser2.Id };

            context.Clients.AddRange(client1, client2);

            var pmUser = new HumanResourcesUser { UserName = "gestor.ana@email.com", Email = "gestor.ana@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(pmUser, "Funcionario@123!");
            await userManager.AddToRoleAsync(pmUser, "Project Manager");
            var employee1 = new Employee { Name = "Ana Silva", Position = "Gestora de Projetos", SpecializationArea = "Software Development", UserId = pmUser.Id };

            var empUser2 = new HumanResourcesUser { UserName = "dev.joao@email.com", Email = "dev.joao@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(empUser2, "Funcionario@123!");
            await userManager.AddToRoleAsync(empUser2, "Employee");
            var employee2 = new Employee { Name = "João Costa", Position = "Developer", SpecializationArea = "Backend", UserId = empUser2.Id };

            var empUser3 = new HumanResourcesUser { UserName = "designer.rita@email.com", Email = "designer.rita@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(empUser3, "Funcionario@123!");
            await userManager.AddToRoleAsync(empUser3, "Employee");
            var employee3 = new Employee { Name = "Rita Pereira", Position = "UI/UX Designer", SpecializationArea = "Design", UserId = empUser3.Id };

            var empUser4 = new HumanResourcesUser { UserName = "dev.carlos@email.com", Email = "dev.carlos@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(empUser4, "Funcionario@123!");
            await userManager.AddToRoleAsync(empUser4, "Employee");
            var employee4 = new Employee { Name = "Carlos Martins", Position = "Backend Developer", SpecializationArea = "C#/.NET", UserId = empUser4.Id };

            var empUser5 = new HumanResourcesUser { UserName = "qa.sofia@email.com", Email = "qa.sofia@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(empUser5, "Funcionario@123!");
            await userManager.AddToRoleAsync(empUser5, "Employee");
            var employee5 = new Employee { Name = "Sofia Costa", Position = "QA Tester", SpecializationArea = "Automation", UserId = empUser5.Id };

            var empUser6 = new HumanResourcesUser { UserName = "devops.hugo@email.com", Email = "devops.hugo@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(empUser6, "Funcionario@123!");
            await userManager.AddToRoleAsync(empUser6, "Employee");
            var employee6 = new Employee { Name = "Hugo Viana", Position = "DevOps Engineer", SpecializationArea = "Cloud/CI-CD", UserId = empUser6.Id };

            context.Employees.AddRange(employee1, employee2, employee3, employee4, employee5, employee6);
            await context.SaveChangesAsync(); // Guardar Clientes e Funcionários para obter os seus IDs

            // --- 2. Criação de Projetos (associados aos Clientes) ---
            var project1 = new Project
            {
                ProjectName = "Novo Website Corporativo",
                Description = "Desenvolvimento do novo portal da Tech Solutions.",
                StartDate = DateTime.Now,
                DueDate = DateTime.Now.AddMonths(3),
                Budget = 25000,
                ProjectStatus = ProjectStatus.InProgress,
                ClientId = client1.Id,
                // REMOVIDO: A associação direta Employee-Project já não existe.
            };

            var project2 = new Project
            {
                ProjectName = "Aplicação Móvel de E-commerce",
                Description = "Criação de uma app para a Innovate Co.",
                StartDate = DateTime.Now.AddDays(15),
                DueDate = DateTime.Now.AddMonths(6),
                Budget = 50000,
                ProjectStatus = ProjectStatus.NotStarted,
                ClientId = client2.Id,
            };

            context.Projects.AddRange(project1, project2);
            await context.SaveChangesAsync(); // Guardar Projetos para obter os seus IDs

            // --- 3. Criação de Contratos (associados aos Projetos e Clientes) ---
            var contract1 = new Contract
            {
                ServiceDescription = "Contrato de Desenvolvimento de Website",
                StartDate = project1.StartDate,
                ExpirationDate = project1.DueDate,
                Value = project1.Budget,
                TermsAndConditions = true,
                ProjectId = project1.Id
            };

            var contract2 = new Contract
            {
                ServiceDescription = "Contrato de Design e Desenvolvimento de App",
                StartDate = project2.StartDate,
                ExpirationDate = project2.DueDate,
                Value = project2.Budget,
                TermsAndConditions = true,
                ProjectId = project2.Id
            };

            context.Contracts.AddRange(contract1, contract2);
            await context.SaveChangesAsync(); // Guardar Contratos para obter os seus IDs

            // --- 4. Associação de Funcionários aos Contratos (a nova lógica) ---
            var empContract1 = new EmployeeContract { EmployeeId = employee1.Id, ContractId = contract1.Id, DurationInDays = 90 };
            var empContract2 = new EmployeeContract { EmployeeId = employee2.Id, ContractId = contract1.Id, DurationInDays = 90 };
            var empContract3 = new EmployeeContract { EmployeeId = employee1.Id, ContractId = contract2.Id, DurationInDays = 180 };
            var empContract4 = new EmployeeContract { EmployeeId = employee3.Id, ContractId = contract2.Id, DurationInDays = 180 };

            context.EmployeeContracts.AddRange(empContract1, empContract2, empContract3, empContract4);

            await context.SaveChangesAsync();
            logger.LogInformation("Base de dados populada com sucesso, seguindo a nova lógica de negócio.");
        }
    }
}

