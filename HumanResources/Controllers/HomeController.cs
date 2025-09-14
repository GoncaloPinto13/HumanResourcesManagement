using System.Diagnostics;               // para Activity (usado em Error para request tracing)
using HumanResources.Models;            // ErrorViewModel
using Microsoft.AspNetCore.Mvc;         // MVC base

namespace HumanResources.Controllers
{
    // Controller default criado pelo template de ASP.NET Core MVC
    // Usado para páginas estáticas como Index, Privacy e Error
    public class HomeController : Controller
    {
        // Logger injetado via DI. Útil para registar eventos, warnings, erros
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger; // injeta instância de logger tipada
        }

        // GET: /
        // Ação principal → mostra página inicial
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/Privacy
        // Página de política de privacidade (exemplo gerado pelo template)
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Error
        // Mostra página de erro com modelo ErrorViewModel
        // [ResponseCache] evita cache para garantir que o erro é sempre atualizado
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                // Se existir Activity (tracing do sistema), usa o Id; caso contrário, usa TraceIdentifier do HttpContext
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
