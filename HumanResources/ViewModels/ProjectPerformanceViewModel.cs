using HumanResources.Models;
using System.ComponentModel.DataAnnotations;

namespace HumanResources.ViewModels
{
    public class ProjectPerformanceViewModel
    {
        public int ProjectId { get; set; }

        [Display(Name = "Nome do Projeto")]
        public string ProjectName { get; set; }

        [Display(Name = "Cliente")]
        public string Cliente { get; set; }

        [Display(Name = "Orçamento (€)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Orcamento { get; set; }

        [Display(Name = "Custo Real (€)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal CustoReal { get; set; }

        [Display(Name = "Tempo Total Previsto (dias)")]
        public int TempoTotalPrevisto { get; set; }

        [Display(Name = "Tempo Total Despendido (dias)")]
        public int TempoTotalDespendido { get; set; }

        [Display(Name = "Total de Funcionários Envolvidos")]
        public int TotalFuncionarios { get; set; }

        [Display(Name = "Status do Projeto")]
        public string Status { get; set; }

        [Display(Name = "Desvio (€)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Desvio => CustoReal - Orcamento;

        // --- PROPRIEDADES ADICIONADAS PARA AS BARRAS DE PROGRESSO ---
        [Display(Name = "% Orçamento Usado")]
        public int PercentagemCusto => Orcamento > 0 ? (int)((CustoReal / Orcamento) * 100) : 0;

        [Display(Name = "% Tempo Decorrido")]
        public int PercentagemTempo => TempoTotalPrevisto > 0 ? (int)(((double)TempoTotalDespendido / TempoTotalPrevisto) * 100) : 0;
    }
}