using HumanResources.Data;
using HumanResources.Models;
using HumanResources.ViewModels; // <-- ADICIONE este using
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- ADICIONE este using
using System.Diagnostics;
using System.Threading.Tasks; // <-- ADICIONE este using

namespace HumanResources.Controllers
{
    public class HomeController : Controller
    {
        private readonly HumanResourcesContext _context;

        // Injeção de dependência do DbContext para aceder à base de dados
        public HomeController(HumanResourcesContext context)
        {
            _context = context;
        }

        // Transformamos o método Index em assíncrono para as consultas
        public async Task<IActionResult> Index()
        {
            // Calcula o número de projetos com o status "InProgress"
            var activeProjects = await _context.Projects
                .CountAsync(p => p.ProjectStatus == ProjectStatus.InProgress);

            // Calcula o número total de funcionários
            var totalEmployees = await _context.Employees.CountAsync();

            // Calcula o número de contratos que expiram nos próximos 30 dias
            var thirtyDaysFromNow = DateTime.Now.AddDays(30);
            var expiringContracts = await _context.Contracts
                .CountAsync(c => c.ExpirationDate > DateTime.Now && c.ExpirationDate <= thirtyDaysFromNow);

            // Cria o ViewModel com os dados calculados
            var viewModel = new DashboardViewModel
            {
                ActiveProjectsCount = activeProjects,
                TotalEmployeesCount = totalEmployees,
                ExpiringContractsCount = expiringContracts
            };

            // Envia o ViewModel para a View
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}