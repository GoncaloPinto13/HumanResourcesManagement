// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
using HumanResources.Areas.Identity.Data;
using HumanResources.Data;
using HumanResources.Data.Seeder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HumanResources
{
    public class Program
    {
        /// <summary>
        /// O método Main é o ponto de entrada da aplicação.
        /// É aqui que a aplicação é configurada e executada.
        /// </summary>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --------------------------------------------------------------------
            // 1. CONFIGURAÇÃO DOS SERVIÇOS (Injeção de Dependência)
            // Nesta secção, registamos todos os serviços que a aplicação
            // irá necessitar, como o acesso à base de dados, sistema de
            // identidade, MVC, etc.
            // --------------------------------------------------------------------

            // Obtém a string de conexão a partir do ficheiro de configuração (appsettings.json).
            var connectionString = builder.Configuration.GetConnectionString("HumanResourcesContextConnection");

            // Regista o DbContext (HumanResourcesContext) no contentor de injeção de dependências.
            // Configura-o para usar o SQL Server com a string de conexão obtida.
            builder.Services.AddDbContext<HumanResourcesContext>(options => options.UseSqlServer(connectionString));

            // Configura o sistema de identidade do ASP.NET Core.
            builder.Services.AddDefaultIdentity<HumanResourcesUser>(options => options.SignIn.RequireConfirmedAccount = false)
                // Adiciona o suporte para perfis (Roles) como "Admin", "Cliente", etc.
                .AddRoles<IdentityRole>()
                // Informa ao Identity para usar o nosso HumanResourcesContext para armazenar os seus dados.
                .AddEntityFrameworkStores<HumanResourcesContext>();

            // Adiciona os serviços necessários para o padrão MVC (Model-View-Controller).
            builder.Services.AddControllersWithViews();
            // Adiciona os serviços necessários para as Razor Pages (usadas pelo Identity UI).
            builder.Services.AddRazorPages();

            // Configura a política de autorização global da aplicação.
            builder.Services.AddAuthorization(options =>
            {
                // Define uma "FallbackPolicy" que se aplica a todos os endpoints que não têm
                // uma política de autorização explícita.
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    // Exige que o utilizador esteja autenticado (logado) para aceder a qualquer página.
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // Constrói a aplicação com todos os serviços configurados.
            var app = builder.Build();


            // --------------------------------------------------------------------
            // 2. CONFIGURAÇÃO DO PIPELINE DE PEDIDOS HTTP (Middleware)
            // Nesta secção, definimos a ordem pela qual os pedidos HTTP
            // serão processados. Cada 'Use...' adiciona um componente (middleware)
            // ao pipeline. A ordem é extremamente importante.
            // --------------------------------------------------------------------

            // Configura o pipeline para ambientes que não são de desenvolvimento.
            if (!app.Environment.IsDevelopment())
            {
                // Redireciona para uma página de erro genérica em caso de exceções não tratadas.
                app.UseExceptionHandler("/Home/Error");
                // Adiciona o cabeçalho HSTS para maior segurança, instruindo os browsers
                // a comunicarem sempre via HTTPS.
                app.UseHsts();
            }

            // Redireciona todos os pedidos HTTP para HTTPS.
            app.UseHttpsRedirection();
            // Permite que a aplicação sirva ficheiros estáticos (CSS, JavaScript, imagens) da pasta wwwroot.
            app.UseStaticFiles();

            // Ativa o sistema de roteamento do ASP.NET Core.
            app.UseRouting();

            // Ativa o middleware de autorização, que aplica as políticas de segurança.
            // Deve vir depois de UseRouting e antes de MapControllerRoute.
            app.UseAuthorization();

            // Mapeia a rota padrão para os controladores MVC.
            // Pattern: {controlador=Home}/{ação=Index}/{id opcional}
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Mapeia as rotas para as Razor Pages (necessário para as páginas de login, registo, etc.).
            app.MapRazorPages();


            // --------------------------------------------------------------------
            // 3. INICIALIZAÇÃO E SEEDING DA BASE DE DADOS
            // --------------------------------------------------------------------

            // Cria um "escopo" de serviço para obter instâncias de serviços
            // necessários para o processo de seeding.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                // Chama o método estático da classe seeder para inicializar a base de dados.
                // Isto aplica migrações e popula a BD com dados iniciais, se necessário.
                await HumanResourcesSeeder.Initialize(services);
            }


            // --------------------------------------------------------------------
            // 4. EXECUÇÃO DA APLICAÇÃO
            // --------------------------------------------------------------------

            // Inicia a aplicação e começa a ouvir por pedidos HTTP.
            app.Run();
        }
    }
}
