// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
using HumanResources.Data;
using HumanResources.Models;
using HumanResources.ViewModels; // Importa os ViewModels, como o DashboardViewModel.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necessário para métodos de consulta assíncronos como CountAsync.
using System.Diagnostics;
using System.Threading.Tasks; // Necessário para programação assíncrona (async/await).

namespace HumanResources.Controllers
{
    /// <summary>
    /// Controlador responsável pelas páginas principais e estáticas da aplicação, como o Dashboard.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly HumanResourcesContext _context;

        /// <summary>
        /// Construtor que injeta o contexto da base de dados (DbContext) para permitir o acesso aos dados.
        /// </summary>
        /// <param name="context">O contexto do Entity Framework Core fornecido pela injeção de dependências.</param>
        public HomeController(HumanResourcesContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ação GET para a rota /Home ou /Home/Index (a página principal da aplicação).
        /// Apresenta o Dashboard com estatísticas agregadas.
        /// O método é assíncrono para não bloquear o servidor enquanto aguarda as consultas à base de dados.
        /// </summary>
        /// <returns>Uma View com o ViewModel do Dashboard preenchido.</returns>
        public async Task<IActionResult> Index()
        {
            // Consulta para calcular o número de projetos com o estado "InProgress".
            // CountAsync é uma operação eficiente que executa um `SELECT COUNT(*)` na base de dados.
            var activeProjects = await _context.Projects
                .CountAsync(p => p.ProjectStatus == ProjectStatus.InProgress);

            // Consulta para calcular o número total de funcionários registados.
            var totalEmployees = await _context.Employees.CountAsync();

            // Calcula a data correspondente a 30 dias a partir do momento atual.
            var thirtyDaysFromNow = DateTime.Now.AddDays(30);
            // Consulta para contar os contratos que expiram no período entre agora e os próximos 30 dias.
            var expiringContracts = await _context.Contracts
                .CountAsync(c => c.ExpirationDate > DateTime.Now && c.ExpirationDate <= thirtyDaysFromNow);

            // Instancia o ViewModel e preenche-o com os dados calculados nas consultas.
            // Usar um ViewModel é uma boa prática que mantém o código limpo e fortemente tipado.
            var viewModel = new DashboardViewModel
            {
                ActiveProjectsCount = activeProjects,
                TotalEmployeesCount = totalEmployees,
                ExpiringContractsCount = expiringContracts
            };

            // Retorna a View "Index.cshtml", passando o ViewModel para que os dados possam ser renderizados em HTML.
            return View(viewModel);
        }

        /// <summary>
        /// Ação GET para a rota /Home/Privacy.
        /// Apresenta a página de Política de Privacidade.
        /// </summary>
        /// <returns>A View "Privacy.cshtml".</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Ação que apresenta uma página de erro genérica para a aplicação.
        /// </summary>
        /// <returns>A View de erro com um identificador único do pedido para depuração.</returns>
        // O atributo [ResponseCache] controla como a resposta desta ação é guardada em cache.
        // As configurações aqui especificadas (Duration = 0, NoStore = true) impedem
        // eficazmente que a página de erro seja guardada em cache pelo browser ou por proxies.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Cria um ErrorViewModel e preenche o RequestId com o identificador da atividade atual,
            // o que é útil para correlacionar logs e depurar o erro.
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}