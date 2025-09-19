// --- IN�CIO DAS IMPORTA��ES (USINGS) ---
using HumanResources.Data;
using HumanResources.Models;
using HumanResources.ViewModels; // Importa os ViewModels, como o DashboardViewModel.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necess�rio para m�todos de consulta ass�ncronos como CountAsync.
using System.Diagnostics;
using System.Threading.Tasks; // Necess�rio para programa��o ass�ncrona (async/await).

namespace HumanResources.Controllers
{
    /// <summary>
    /// Controlador respons�vel pelas p�ginas principais e est�ticas da aplica��o, como o Dashboard.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly HumanResourcesContext _context;

        /// <summary>
        /// Construtor que injeta o contexto da base de dados (DbContext) para permitir o acesso aos dados.
        /// </summary>
        /// <param name="context">O contexto do Entity Framework Core fornecido pela inje��o de depend�ncias.</param>
        public HomeController(HumanResourcesContext context)
        {
            _context = context;
        }

        /// <summary>
        /// A��o GET para a rota /Home ou /Home/Index (a p�gina principal da aplica��o).
        /// Apresenta o Dashboard com estat�sticas agregadas.
        /// O m�todo � ass�ncrono para n�o bloquear o servidor enquanto aguarda as consultas � base de dados.
        /// </summary>
        /// <returns>Uma View com o ViewModel do Dashboard preenchido.</returns>
        public async Task<IActionResult> Index()
        {
            // Consulta para calcular o n�mero de projetos com o estado "InProgress".
            // CountAsync � uma opera��o eficiente que executa um `SELECT COUNT(*)` na base de dados.
            var activeProjects = await _context.Projects
                .CountAsync(p => p.ProjectStatus == ProjectStatus.InProgress);

            // Consulta para calcular o n�mero total de funcion�rios registados.
            var totalEmployees = await _context.Employees.CountAsync();

            // Calcula a data correspondente a 30 dias a partir do momento atual.
            var thirtyDaysFromNow = DateTime.Now.AddDays(30);
            // Consulta para contar os contratos que expiram no per�odo entre agora e os pr�ximos 30 dias.
            var expiringContracts = await _context.Contracts
                .CountAsync(c => c.ExpirationDate > DateTime.Now && c.ExpirationDate <= thirtyDaysFromNow);

            // Instancia o ViewModel e preenche-o com os dados calculados nas consultas.
            // Usar um ViewModel � uma boa pr�tica que mant�m o c�digo limpo e fortemente tipado.
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
        /// A��o GET para a rota /Home/Privacy.
        /// Apresenta a p�gina de Pol�tica de Privacidade.
        /// </summary>
        /// <returns>A View "Privacy.cshtml".</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// A��o que apresenta uma p�gina de erro gen�rica para a aplica��o.
        /// </summary>
        /// <returns>A View de erro com um identificador �nico do pedido para depura��o.</returns>
        // O atributo [ResponseCache] controla como a resposta desta a��o � guardada em cache.
        // As configura��es aqui especificadas (Duration = 0, NoStore = true) impedem
        // eficazmente que a p�gina de erro seja guardada em cache pelo browser ou por proxies.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Cria um ErrorViewModel e preenche o RequestId com o identificador da atividade atual,
            // o que � �til para correlacionar logs e depurar o erro.
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}