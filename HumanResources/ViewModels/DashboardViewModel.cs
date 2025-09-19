// O namespace agrupa os ViewModels, separando-os dos modelos de dados do domínio.
namespace HumanResources.ViewModels
{
    /// <summary>
    /// ViewModel para a view do Dashboard (página principal).
    /// Agrega e transporta os principais indicadores (KPIs) a serem exibidos.
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// Armazena o número total de projetos que estão atualmente ativos (em andamento).
        /// </summary>
        public int ActiveProjectsCount { get; set; }

        /// <summary>
        /// Armazena o número total de funcionários registados no sistema.
        /// </summary>
        public int TotalEmployeesCount { get; set; }

        /// <summary>
        /// Armazena o número de contratos que estão prestes a expirar (ex: nos próximos 30 dias).
        /// </summary>
        public int ExpiringContractsCount { get; set; }
    }
}