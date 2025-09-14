using System.ComponentModel.DataAnnotations;

namespace HumanResources.ViewModels
{
    public class ProjectPerformanceViewModel
    {
        public int ProjectId { get; set; }

        [Display(Name = "Nome do Projeto")]
        public string ProjectName { get; set; } = default!; // Adicione '= default!'
                                                            // ...
        [Display(Name = "Orçamento (€)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Orcamento { get; set; }

        [Display(Name = "Custo Real Estimado (€)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal CustoReal { get; set; }

        [Display(Name = "Desvio (€)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Desvio => CustoReal - Orcamento;

        [Display(Name = "Tempo Previsto (dias)")]
        public int TempoTotalPrevisto { get; set; }

        [Display(Name = "Tempo Despendido (dias)")]
        public int TempoDespendido { get; set; }
    }
}
