// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
// Importa atributos para validação e formatação de dados.
using System.ComponentModel.DataAnnotations;

// O namespace agrupa os ViewModels, separando-os dos modelos de dados do domínio.
namespace HumanResources.ViewModels
{
    /// <summary>
    /// ViewModel para apresentar os dados de performance de um projeto.
    /// Esta classe é o modelo para os dados retornados pela Stored Procedure
    /// `sp_GetProjectPerformanceReport` e contém propriedades calculadas
    /// para facilitar a exibição de métricas na View.
    /// </summary>
    public class ProjectPerformanceViewModel
    {
        /// <summary>
        /// Identificador único do projeto.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Nome do projeto.
        /// O atributo [Display] personaliza o nome do campo na UI (ex: cabeçalho de uma tabela).
        /// </summary>
        [Display(Name = "Nome do Projeto")]
        public string ProjectName { get; set; }

        /// <summary>
        /// O valor orçamentado para o projeto.
        /// O atributo [DisplayFormat] formata o valor para exibição.
        /// "{0:C}" formata o número como uma moeda, usando o símbolo da cultura local (ex: €).
        /// </summary>
        [Display(Name = "Orçamento (€)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Orcamento { get; set; }

        /// <summary>
        /// O custo real estimado ou já despendido no projeto.
        /// </summary>
        [Display(Name = "Custo Real Estimado (€)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal CustoReal { get; set; }

        /// <summary>
        /// Propriedade calculada que mostra a diferença entre o custo real e o orçamento.
        /// Um valor positivo indica que o custo excedeu o orçamento.
        /// A sintaxe "=>" define um getter "expression-bodied", uma forma concisa de criar uma propriedade só de leitura.
        /// </summary>
        [Display(Name = "Desvio (€)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Desvio => CustoReal - Orcamento;

        /// <summary>
        /// O tempo total, em dias, previsto para a conclusão do projeto.
        /// </summary>
        [Display(Name = "Tempo Previsto (dias)")]
        public int TempoTotalPrevisto { get; set; }

        /// <summary>
        /// O tempo, em dias, já despendido no projeto.
        /// </summary>
        [Display(Name = "Tempo Despendido (dias)")]
        public int TempoDespendido { get; set; }

        // --- PROPRIEDADES ADICIONADAS PARA AS BARRAS DE PROGRESSO ---

        /// <summary>
        /// Propriedade calculada que determina a percentagem do orçamento já utilizado.
        /// Usa um operador ternário para evitar uma divisão por zero se o orçamento for 0.
        /// Sintaxe: (condição ? valor_se_verdadeiro : valor_se_falso)
        /// </summary>
        [Display(Name = "% Orçamento Usado")]
        public int PercentagemCusto => Orcamento > 0 ? (int)((CustoReal / Orcamento) * 100) : 0;

        /// <summary>
        /// Propriedade calculada que determina a percentagem do tempo previsto que já decorreu.
        /// O "(double)" é um "cast" que força a divisão a ser feita com casas decimais,
        /// garantindo um cálculo de percentagem correto antes de converter o resultado para inteiro.
        /// </summary>
        [Display(Name = "% Tempo Decorrido")]
        public int PercentagemTempo => TempoTotalPrevisto > 0 ? (int)(((double)TempoDespendido / TempoTotalPrevisto) * 100) : 0;
    }
}