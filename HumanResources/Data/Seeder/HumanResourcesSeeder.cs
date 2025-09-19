// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
// Importa classes relacionadas com a identidade do utilizador (HumanResourcesUser).
using HumanResources.Areas.Identity.Data;
// Importa o contexto da base de dados (HumanResourcesContext).
using HumanResources.Data;
// Importa os modelos de dados da aplicação (Client, Employee, etc.).
using HumanResources.Models;
// Importa as classes principais do ASP.NET Core Identity (UserManager, RoleManager, IdentityRole).
using Microsoft.AspNetCore.Identity;
// Importa funcionalidades do Entity Framework Core, como o método MigrateAsync.
using Microsoft.EntityFrameworkCore;
// Importa funcionalidades para a injeção de dependências (IServiceProvider, CreateScope).
using Microsoft.Extensions.DependencyInjection;
// Importa a interface de logging para registar informações e erros.
using Microsoft.Extensions.Logging;
// Importa tipos básicos do .NET.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// --- FIM DAS IMPORTAÇÕES ---

// O namespace organiza o código, agrupando classes relacionadas.
namespace HumanResources.Data.Seeder
{
    /// <summary>
    /// Classe estática responsável por inicializar e popular a base de dados da aplicação.
    /// Garante que a estrutura da BD está atualizada (migrações), que os perfis (roles) essenciais
    /// e o utilizador administrador existem, e que a BD é preenchida com dados de exemplo
    /// para facilitar o desenvolvimento e os testes.
    /// </summary>
    public class HumanResourcesSeeder
    {
        /// <summary>
        /// O ponto de entrada principal do seeder. Este método orquestra todo o processo.
        /// É chamado na inicialização da aplicação (normalmente no ficheiro Program.cs).
        /// </summary>
        /// <param name="serviceProvider">
        /// Um objeto que fornece acesso ao contentor de injeção de dependências da aplicação,
        /// permitindo obter instâncias de serviços como o DbContext, UserManager, etc.
        /// </param>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // 'CreateScope' cria um novo "escopo" de serviço. É uma boa prática usar escopos
            // para garantir que os serviços com tempo de vida "scoped" (como o DbContext)
            // sejam criados e descartados corretamente, evitando problemas de memória ou de concorrência.
            using (var scope = serviceProvider.CreateScope())
            {
                // Obtém as instâncias dos serviços necessários dentro do escopo atual.
                // 'GetRequiredService' lança uma exceção se o serviço não estiver registado,
                // o que ajuda a detetar erros de configuração mais cedo.
                var context = scope.ServiceProvider.GetRequiredService<HumanResourcesContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<HumanResourcesUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<HumanResourcesSeeder>>();

                // Um bloco try-catch é usado para capturar e registar quaisquer exceções
                // que possam ocorrer durante o processo de seeding, tornando a aplicação mais robusta.
                try
                {
                    logger.LogInformation("A aplicar migrações da base de dados...");
                    // Garante que todas as migrações pendentes do Entity Framework Core são aplicadas.
                    // Isto cria as tabelas e atualiza o esquema da base de dados para a versão mais recente.
                    // É o primeiro passo crucial antes de tentar inserir dados.
                    await context.Database.MigrateAsync();

                    // Chama o método para criar os perfis (roles) e o utilizador administrador.
                    // Esta lógica é separada para melhor organização e é executada sempre,
                    // pois os perfis e o admin são fundamentais para o funcionamento da aplicação.
                    await SeedRolesAndAdminAsync(userManager, roleManager, logger);

                    // VERIFICAÇÃO DE IDEMPOTÊNCIA:
                    // Verifica se a tabela de Clientes já tem algum registo.
                    // Esta é a verificação principal para decidir se os dados de negócio devem ser inseridos.
                    // Isto impede que os dados sejam duplicados cada vez que a aplicação é iniciada.
                    if (!context.Clients.Any())
                    {
                        logger.LogInformation("A popular a base de dados com dados de negócio...");
                        // Se não houver clientes, chama o método que insere todos os dados de exemplo
                        // (clientes, funcionários, projetos, etc.).
                        await SeedBusinessDataAsync(context, userManager, logger);
                    }
                    else
                    {
                        // Se já existirem dados, informa no log que o seeding foi ignorado.
                        logger.LogInformation("A base de dados já contém dados de negócio. Seeding ignorado.");
                    }
                }
                catch (Exception ex)
                {
                    // Se ocorrer qualquer erro durante o processo, regista-o detalhadamente.
                    logger.LogError(ex, "Ocorreu um erro durante o seeding da base de dados.");
                }
            }
        }

        /// <summary>
        /// Cria os perfis (roles) essenciais da aplicação e o utilizador administrador.
        /// Este método é idempotente: só cria os perfis e o utilizador se eles ainda não existirem.
        /// </summary>
        private static async Task SeedRolesAndAdminAsync(UserManager<HumanResourcesUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<HumanResourcesSeeder> logger)
        {
            // Define a lista de nomes de perfis que a aplicação necessita.
            string[] roleNames = { "Admin", "Project Manager", "Employee", "Cliente" };

            // Itera sobre cada nome de perfil.
            foreach (var roleName in roleNames)
            {
                // Verifica se o perfil já existe na base de dados.
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    // Se não existir, cria um novo objeto IdentityRole e guarda-o na base de dados.
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation($"Perfil '{roleName}' criado.");
                }
            }

            // Define o email do utilizador administrador.
            var adminEmail = "admin@hr.com";
            // Verifica se já existe um utilizador com este email.
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                // Se não existir, cria uma nova instância do utilizador.
                // 'EmailConfirmed = true' é útil para saltar o passo de confirmação de email para o admin.
                var adminUser = new HumanResourcesUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };

                // Tenta criar o utilizador com a password definida.
                var result = await userManager.CreateAsync(adminUser, "Admin@123!");

                // Se a criação do utilizador for bem-sucedida...
                if (result.Succeeded)
                {
                    // ...associa o utilizador recém-criado ao perfil "Admin".
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Utilizador Admin criado e associado ao perfil 'Admin'.");
                }
            }
        }

        /// <summary>
        /// Popula a base de dados com um conjunto de dados de negócio de exemplo.
        /// A ordem de criação é fundamental para respeitar as chaves estrangeiras.
        /// 1. Utilizadores, Clientes e Funcionários.
        /// 2. Projetos (que dependem de Clientes).
        /// 3. Contratos (que dependem de Projetos).
        /// 4. EmployeeContracts (a tabela de junção, que depende de Funcionários e Contratos).
        /// </summary>
        private static async Task SeedBusinessDataAsync(HumanResourcesContext context, UserManager<HumanResourcesUser> userManager, ILogger<HumanResourcesSeeder> logger)
        {
            // --- PASSO 1: CRIAÇÃO DE CLIENTES E FUNCIONÁRIOS (E OS SEUS UTILIZADORES ASSOCIADOS) ---

            // Cliente 1
            var clientUser1 = new HumanResourcesUser { UserName = "cliente.tech@email.com", Email = "cliente.tech@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(clientUser1, "Cliente@123!");
            await userManager.AddToRoleAsync(clientUser1, "Cliente");
            var client1 = new Client { CompanyName = "Tech Solutions", Nif = "508123456", UserId = clientUser1.Id };

            // Cliente 2
            var clientUser2 = new HumanResourcesUser { UserName = "cliente.innovate@email.com", Email = "cliente.innovate@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(clientUser2, "Cliente@123!");
            await userManager.AddToRoleAsync(clientUser2, "Cliente");
            var client2 = new Client { CompanyName = "Innovate Co.", Nif = "509123789", UserId = clientUser2.Id };

            // Adiciona os objetos Cliente ao contexto do EF Core para serem inseridos na BD.
            context.Clients.AddRange(client1, client2);

            // Funcionário 1 (Gestor de Projeto)
            var pmUser = new HumanResourcesUser { UserName = "gestor.ana@email.com", Email = "gestor.ana@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(pmUser, "Funcionario@123!");
            await userManager.AddToRoleAsync(pmUser, "Project Manager");
            var employee1 = new Employee { Name = "Ana Silva", Position = "Gestora de Projetos", SpecializationArea = "Software Development", UserId = pmUser.Id };

            // Funcionário 2 (Developer)
            var empUser2 = new HumanResourcesUser { UserName = "dev.joao@email.com", Email = "dev.joao@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(empUser2, "Funcionario@123!");
            await userManager.AddToRoleAsync(empUser2, "Employee");
            var employee2 = new Employee { Name = "João Costa", Position = "Developer", SpecializationArea = "Backend", UserId = empUser2.Id };

            // Funcionário 3 (Designer)
            var empUser3 = new HumanResourcesUser { UserName = "designer.rita@email.com", Email = "designer.rita@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(empUser3, "Funcionario@123!");
            await userManager.AddToRoleAsync(empUser3, "Employee");
            var employee3 = new Employee { Name = "Rita Pereira", Position = "UI/UX Designer", SpecializationArea = "Design", UserId = empUser3.Id };

            // ... (Criação dos restantes funcionários de forma semelhante) ...
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

            // Adiciona os funcionários ao contexto.
            context.Employees.AddRange(employee1, employee2, employee3, employee4, employee5, employee6);

            // Guarda as alterações na base de dados.
            // É CRUCIAL fazer isto aqui para que o EF Core gere os IDs (chaves primárias)
            // para os Clientes e Funcionários, que serão usados como chaves estrangeiras nos passos seguintes.
            await context.SaveChangesAsync();

            // --- PASSO 2: CRIAÇÃO DE PROJETOS (associados aos Clientes) ---
            var project1 = new Project
            {
                ProjectName = "Novo Website Corporativo",
                Description = "Desenvolvimento do novo portal da Tech Solutions.",
                StartDate = DateTime.Now,
                DueDate = DateTime.Now.AddMonths(3),
                Budget = 25000,
                ProjectStatus = ProjectStatus.InProgress,
                ClientId = client1.Id, // Usa o ID do client1 que foi gerado no passo anterior.
            };

            var project2 = new Project
            {
                ProjectName = "Aplicação Móvel de E-commerce",
                Description = "Criação de uma app para a Innovate Co.",
                StartDate = DateTime.Now.AddDays(15),
                DueDate = DateTime.Now.AddMonths(6),
                Budget = 50000,
                ProjectStatus = ProjectStatus.NotStarted,
                ClientId = client2.Id, // Usa o ID do client2.
            };

            context.Projects.AddRange(project1, project2);
            // Guarda novamente para que os Projetos obtenham os seus IDs.
            await context.SaveChangesAsync();

            // --- PASSO 3: CRIAÇÃO DE CONTRATOS (associados aos Projetos) ---
            var contract1 = new Contract
            {
                ServiceDescription = "Contrato de Desenvolvimento de Website",
                StartDate = project1.StartDate,
                ExpirationDate = project1.DueDate,
                Value = project1.Budget,
                TermsAndConditions = true,
                ProjectId = project1.Id // Usa o ID do project1.
            };

            var contract2 = new Contract
            {
                ServiceDescription = "Contrato de Design e Desenvolvimento de App",
                StartDate = project2.StartDate,
                ExpirationDate = project2.DueDate,
                Value = project2.Budget,
                TermsAndConditions = true,
                ProjectId = project2.Id // Usa o ID do project2.
            };

            context.Contracts.AddRange(contract1, contract2);
            // Guarda para que os Contratos obtenham os seus IDs.
            await context.SaveChangesAsync();

            // --- PASSO 4: ASSOCIAÇÃO DE FUNCIONÁRIOS AOS CONTRATOS (povoamento da tabela de junção) ---
            // Aqui criamos as relações Muitos-para-Muitos.
            var empContract1 = new EmployeeContract { EmployeeId = employee1.Id, ContractId = contract1.Id, DurationInDays = 90 };
            var empContract2 = new EmployeeContract { EmployeeId = employee2.Id, ContractId = contract1.Id, DurationInDays = 90 };
            var empContract3 = new EmployeeContract { EmployeeId = employee1.Id, ContractId = contract2.Id, DurationInDays = 180 }; // O mesmo funcionário pode estar em vários contratos.
            var empContract4 = new EmployeeContract { EmployeeId = employee3.Id, ContractId = contract2.Id, DurationInDays = 180 };

            context.EmployeeContracts.AddRange(empContract1, empContract2, empContract3, empContract4);

            // Guarda todas as alterações finais na base de dados.
            await context.SaveChangesAsync();
            logger.LogInformation("Base de dados populada com sucesso, seguindo a nova lógica de negócio.");
        }
    }
}