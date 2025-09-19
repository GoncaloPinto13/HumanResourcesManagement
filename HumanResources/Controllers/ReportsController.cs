// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
using HumanResources.Data;
using HumanResources.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Importa os serviços do Identity, como o UserManager.
using HumanResources.Areas.Identity.Data; // Importa a classe de utilizador personalizada.
using Microsoft.Data.SqlClient; // Importa a classe SqlParameter para passar parâmetros de forma segura para SQL.


namespace HumanResources.Controllers
{
    /// <summary>
    /// Controlador responsável pela geração e exibição de relatórios.
    /// </summary>
    public class ReportsController : Controller
    {
        // --- INJEÇÃO DE DEPENDÊNCIAS ---
        private readonly HumanResourcesContext _context;
        private readonly UserManager<HumanResourcesUser> _userManager;

        /// <summary>
        /// Construtor que injeta as dependências necessárias.
        /// </summary>
        /// <param name="context">O contexto da base de dados para executar consultas.</param>
        /// <param name="userManager">O serviço para gerir e obter informações sobre os utilizadores logados.</param>
        public ReportsController(HumanResourcesContext context, UserManager<HumanResourcesUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Ação GET para a rota /Reports/ProjectPerformance.
        /// Gera e exibe o relatório de performance de projetos, aplicando filtros de pesquisa e segurança.
        /// </summary>
        /// <param name="searchString">Termo de pesquisa opcional para filtrar projetos pelo nome.</param>
        public async Task<IActionResult> ProjectPerformance(string searchString)
        {
            // Guarda o filtro de pesquisa atual no ViewData para que o campo de texto na View
            // possa ser repopulado com o valor que o utilizador inseriu.
            ViewData["CurrentFilter"] = searchString;

            // --- PREPARAÇÃO DOS PARÂMETROS PARA A STORED PROCEDURE ---
            // Cria os parâmetros que serão passados para a Stored Procedure.
            // É crucial usar a classe SqlParameter para prevenir ataques de SQL Injection.

            // Parâmetro para o ID do cliente. Inicializado com DBNull.Value por defeito,
            // o que significa "nenhum filtro de cliente".
            var clientIdParam = new SqlParameter("@ClientId", DBNull.Value);

            // Parâmetro para a string de pesquisa. Usa um operador ternário:
            // Se a searchString for nula ou vazia, passa DBNull.Value.
            // Caso contrário, passa o valor da string.
            var searchParam = new SqlParameter("@SearchString", string.IsNullOrEmpty(searchString) ? DBNull.Value : (object)searchString);

            // --- LÓGICA DE SEGURANÇA BASEADA NO PERFIL (ROLE-BASED SECURITY) ---
            // Verifica se o utilizador atualmente logado tem o perfil "Cliente".
            if (User.IsInRole("Cliente"))
            {
                // Se for um cliente, obtém o objeto completo do utilizador logado.
                var user = await _userManager.GetUserAsync(User);

                // Verifica se o utilizador tem um ClientId associado (garante que não é nulo).
                if (user.ClientId.HasValue)
                {
                    // Se tiver, atualiza o valor do parâmetro @ClientId para o ID do cliente do utilizador.
                    // Isto garante que a Stored Procedure só retornará projetos deste cliente específico.
                    clientIdParam.Value = user.ClientId.Value;
                }
            }

            // --- EXECUÇÃO DA STORED PROCEDURE ---
            // Executa a Stored Procedure "sp_GetProjectPerformanceReport" na base de dados.
            // Os resultados da procedure são mapeados para uma lista de 'ProjectPerformanceViewModel'.
            // O `FromSqlRaw` permite executar SQL bruto, e os parâmetros são passados de forma segura.
            var reportData = await _context.ProjectPerformanceReport
                .FromSqlRaw("EXEC dbo.sp_GetProjectPerformanceReport @ClientId, @SearchString", clientIdParam, searchParam)
                .ToListAsync();

            // Retorna a View, passando a lista de dados do relatório para ser renderizada.
            return View(reportData);
        }
    }
}
