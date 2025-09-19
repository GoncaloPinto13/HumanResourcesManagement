// --- IN�CIO DAS IMPORTA��ES (USINGS) ---
// Importa classes relacionadas com a identidade do utilizador (HumanResourcesUser).
using HumanResources.Areas.Identity.Data;
// Importa o contexto da base de dados (HumanResourcesContext).
using HumanResources.Data;
// Importa os modelos de dados da aplica��o (Client, Employee, etc.).
using HumanResources.Models;
// Importa as classes principais do ASP.NET Core Identity (UserManager, RoleManager, IdentityRole).
using Microsoft.AspNetCore.Identity;
// Importa funcionalidades do Entity Framework Core, como o m�todo MigrateAsync.
using Microsoft.EntityFrameworkCore;
// Importa funcionalidades para a inje��o de depend�ncias (IServiceProvider, CreateScope).
using Microsoft.Extensions.DependencyInjection;
// Importa a interface de logging para registar informa��es e erros.
using Microsoft.Extensions.Logging;
// Importa tipos b�sicos do .NET.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// --- FIM DAS IMPORTA��ES ---

// O namespace organiza o c�digo, agrupando classes relacionadas.
namespace HumanResources.Data.Seeder
{
    /// <summary>
    /// Classe est�tica respons�vel por inicializar e popular a base de dados da aplica��o.
    /// Garante que a estrutura da BD est� atualizada (migra��es), que os perfis (roles) essenciais
    /// e o utilizador administrador existem, e que a BD � preenchida com dados de exemplo
    /// para facilitar o desenvolvimento e os testes.
    /// </summary>
    public class HumanResourcesSeeder
    {
        /// <summary>
        /// O ponto de entrada principal do seeder. Este m�todo orquestra todo o processo.
        /// � chamado na inicializa��o da aplica��o (normalmente no ficheiro Program.cs).
        /// </summary>
        /// <param name="serviceProvider">
        /// Um objeto que fornece acesso ao contentor de inje��o de depend�ncias da aplica��o,
        /// permitindo obter inst�ncias de servi�os como o DbContext, UserManager, etc.
        /// </param>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // 'CreateScope' cria um novo "escopo" de servi�o. � uma boa pr�tica usar escopos
            // para garantir que os servi�os com tempo de vida "scoped" (como o DbContext)
            // sejam criados e descartados corretamente, evitando problemas de mem�ria ou de concorr�ncia.
            using (var scope = serviceProvider.CreateScope())
            {
                // Obt�m as inst�ncias dos servi�os necess�rios dentro do escopo atual.
                // 'GetRequiredService' lan�a uma exce��o se o servi�o n�o estiver registado,
                // o que ajuda a detetar erros de configura��o mais cedo.
                var context = scope.ServiceProvider.GetRequiredService<HumanResourcesContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<HumanResourcesUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<HumanResourcesSeeder>>();

                // Um bloco try-catch � usado para capturar e registar quaisquer exce��es
                // que possam ocorrer durante o processo de seeding, tornando a aplica��o mais robusta.
                try
                {
                    logger.LogInformation("A aplicar migra��es da base de dados...");
                    // Garante que todas as migra��es pendentes do Entity Framework Core s�o aplicadas.
                    // Isto cria as tabelas e atualiza o esquema da base de dados para a vers�o mais recente.
                    // � o primeiro passo crucial antes de tentar inserir dados.
                    await context.Database.MigrateAsync();

                    // Chama o m�todo para criar os perfis (roles) e o utilizador administrador.
                    // Esta l�gica � separada para melhor organiza��o e � executada sempre,
                    // pois os perfis e o admin s�o fundamentais para o funcionamento da aplica��o.
                    await SeedRolesAndAdminAsync(userManager, roleManager, logger);

                    // VERIFICA��O DE IDEMPOT�NCIA:
                    // Verifica se a tabela de Clientes j� tem algum registo.
                    // Esta � a verifica��o principal para decidir se os dados de neg�cio devem ser inseridos.
                    // Isto impede que os dados sejam duplicados cada vez que a aplica��o � iniciada.
                    if (!context.Clients.Any())
                    {
                        logger.LogInformation("A popular a base de dados com dados de neg�cio...");
                        // Se n�o houver clientes, chama o m�todo que insere todos os dados de exemplo
                        // (clientes, funcion�rios, projetos, etc.).
                        await SeedBusinessDataAsync(context, userManager, logger);
                    }
                    else
                    {
                        // Se j� existirem dados, informa no log que o seeding foi ignorado.
                        logger.LogInformation("A base de dados j� cont�m dados de neg�cio. Seeding ignorado.");
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
        /// Cria os perfis (roles) essenciais da aplica��o e o utilizador administrador.
        /// Este m�todo � idempotente: s� cria os perfis e o utilizador se eles ainda n�o existirem.
        /// </summary>
        private static async Task SeedRolesAndAdminAsync(UserManager<HumanResourcesUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<HumanResourcesSeeder> logger)
        {
            // Define a lista de nomes de perfis que a aplica��o necessita.
            string[] roleNames = { "Admin", "Project Manager", "Employee", "Cliente" };

            // Itera sobre cada nome de perfil.
            foreach (var roleName in roleNames)
            {
                // Verifica se o perfil j� existe na base de dados.
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    // Se n�o existir, cria um novo objeto IdentityRole e guarda-o na base de dados.
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation($"Perfil '{roleName}' criado.");
                }
            }

            // Define o email do utilizador administrador.
            var adminEmail = "admin@hr.com";
            // Verifica se j� existe um utilizador com este email.
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                // Se n�o existir, cria uma nova inst�ncia do utilizador.
                // 'EmailConfirmed = true' � �til para saltar o passo de confirma��o de email para o admin.
                var adminUser = new HumanResourcesUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };

                // Tenta criar o utilizador com a password definida.
                var result = await userManager.CreateAsync(adminUser, "Admin@123!");

                // Se a cria��o do utilizador for bem-sucedida...
                if (result.Succeeded)
                {
                    // ...associa o utilizador rec�m-criado ao perfil "Admin".
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Utilizador Admin criado e associado ao perfil 'Admin'.");
                }
            }
        }

        /// <summary>
        /// Popula a base de dados com um conjunto de dados de neg�cio de exemplo.
        /// A ordem de cria��o � fundamental para respeitar as chaves estrangeiras.
        /// 1. Utilizadores, Clientes e Funcion�rios.
        /// 2. Projetos (que dependem de Clientes).
        /// 3. Contratos (que dependem de Projetos).
        /// 4. EmployeeContracts (a tabela de jun��o, que depende de Funcion�rios e Contratos).
        /// </summary>
        private static async Task SeedBusinessDataAsync(HumanResourcesContext context, UserManager<HumanResourcesUser> userManager, ILogger<HumanResourcesSeeder> logger)
        {
            // --- PASSO 1: CRIA��O DE CLIENTES E FUNCION�RIOS (E OS SEUS UTILIZADORES ASSOCIADOS) ---

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

            // Funcion�rio 1 (Gestor de Projeto)
            var pmUser = new HumanResourcesUser { UserName = "gestor.ana@email.com", Email = "gestor.ana@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(pmUser, "Funcionario@123!");
            await userManager.AddToRoleAsync(pmUser, "Project Manager");
            var employee1 = new Employee { Name = "Ana Silva", Position = "Gestora de Projetos", SpecializationArea = "Software Development", UserId = pmUser.Id };

            // Funcion�rio 2 (Developer)
            var empUser2 = new HumanResourcesUser { UserName = "dev.joao@email.com", Email = "dev.joao@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(empUser2, "Funcionario@123!");
            await userManager.AddToRoleAsync(empUser2, "Employee");
            var employee2 = new Employee { Name = "Jo�o Costa", Position = "Developer", SpecializationArea = "Backend", UserId = empUser2.Id };

            // Funcion�rio 3 (Designer)
            var empUser3 = new HumanResourcesUser { UserName = "designer.rita@email.com", Email = "designer.rita@email.com", EmailConfirmed = true };
            await userManager.CreateAsync(empUser3, "Funcionario@123!");
            await userManager.AddToRoleAsync(empUser3, "Employee");
            var employee3 = new Employee { Name = "Rita Pereira", Position = "UI/UX Designer", SpecializationArea = "Design", UserId = empUser3.Id };

            // ... (Cria��o dos restantes funcion�rios de forma semelhante) ...
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

            // Adiciona os funcion�rios ao contexto.
            context.Employees.AddRange(employee1, employee2, employee3, employee4, employee5, employee6);

            // Guarda as altera��es na base de dados.
            // � CRUCIAL fazer isto aqui para que o EF Core gere os IDs (chaves prim�rias)
            // para os Clientes e Funcion�rios, que ser�o usados como chaves estrangeiras nos passos seguintes.
            await context.SaveChangesAsync();

            // --- PASSO 2: CRIA��O DE PROJETOS (associados aos Clientes) ---
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
                ProjectName = "Aplica��o M�vel de E-commerce",
                Description = "Cria��o de uma app para a Innovate Co.",
                StartDate = DateTime.Now.AddDays(15),
                DueDate = DateTime.Now.AddMonths(6),
                Budget = 50000,
                ProjectStatus = ProjectStatus.NotStarted,
                ClientId = client2.Id, // Usa o ID do client2.
            };

            context.Projects.AddRange(project1, project2);
            // Guarda novamente para que os Projetos obtenham os seus IDs.
            await context.SaveChangesAsync();

            // --- PASSO 3: CRIA��O DE CONTRATOS (associados aos Projetos) ---
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

            // --- PASSO 4: ASSOCIA��O DE FUNCION�RIOS AOS CONTRATOS (povoamento da tabela de jun��o) ---
            // Aqui criamos as rela��es Muitos-para-Muitos.
            var empContract1 = new EmployeeContract { EmployeeId = employee1.Id, ContractId = contract1.Id, DurationInDays = 90 };
            var empContract2 = new EmployeeContract { EmployeeId = employee2.Id, ContractId = contract1.Id, DurationInDays = 90 };
            var empContract3 = new EmployeeContract { EmployeeId = employee1.Id, ContractId = contract2.Id, DurationInDays = 180 }; // O mesmo funcion�rio pode estar em v�rios contratos.
            var empContract4 = new EmployeeContract { EmployeeId = employee3.Id, ContractId = contract2.Id, DurationInDays = 180 };

            context.EmployeeContracts.AddRange(empContract1, empContract2, empContract3, empContract4);

            // Guarda todas as altera��es finais na base de dados.
            await context.SaveChangesAsync();
            logger.LogInformation("Base de dados populada com sucesso, seguindo a nova l�gica de neg�cio.");
        }
    }
}