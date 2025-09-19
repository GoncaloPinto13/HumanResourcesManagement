using HumanResources.Data;
using HumanResources.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Adicionado
using HumanResources.Areas.Identity.Data; // Adicionado
using Microsoft.Data.SqlClient; // Adicionado para os parâmetros


namespace HumanResources.Controllers
{
    public class ReportsController : Controller
    {
        private readonly HumanResourcesContext _context;
        private readonly UserManager<HumanResourcesUser> _userManager; // Adicionado

        // Construtor atualizado para injetar o UserManager
        public ReportsController(HumanResourcesContext context, UserManager<HumanResourcesUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reports/ProjectPerformance
        // Ação modificada para aceitar filtros e aplicar segurança
        public async Task<IActionResult> ProjectPerformance(string searchString)
        {
            ViewData["CurrentFilter"] = searchString; // Para manter o valor no campo de pesquisa

            var clientIdParam = new SqlParameter("@ClientId", DBNull.Value);
            var searchParam = new SqlParameter("@SearchString", string.IsNullOrEmpty(searchString) ? DBNull.Value : searchString);

            // Lógica de segurança baseada no perfil
            if (User.IsInRole("Cliente"))
            {
                var user = await _userManager.GetUserAsync(User);
                // NOTA: Isto assume que o seu HumanResourcesUser tem uma propriedade ClientId
                // Se não tiver, terá de a adicionar ou obter o ID do cliente de outra forma.
                if (user.ClientId.HasValue)
                {
                    clientIdParam.Value = user.ClientId.Value;
                }
            }

            var reportData = await _context.ProjectPerformanceReport
                .FromSqlRaw("EXEC dbo.sp_GetProjectPerformanceReport @ClientId, @SearchString", clientIdParam, searchParam)
                .ToListAsync();

            return View(reportData);
        }
    }
}