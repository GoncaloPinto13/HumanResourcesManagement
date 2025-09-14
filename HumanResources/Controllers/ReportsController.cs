using HumanResources.Data;
using HumanResources.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HumanResources.Controllers
{
    public class ReportsController : Controller
    {
        private readonly HumanResourcesContext _context;

        public ReportsController(HumanResourcesContext context)
        {
            _context = context;
        }

        // GET: Reports/ProjectPerformance
        public async Task<IActionResult> ProjectPerformance()
        {
            // --- ESTA É A LINHA CORRIGIDA ---
            // Usamos a propriedade DbSet que definimos no DbContext
            var reportData = await _context.ProjectPerformanceReport
                .FromSqlRaw("EXEC dbo.sp_GetProjectPerformanceReport")
                .ToListAsync();

            return View(reportData);
        }
    }
}