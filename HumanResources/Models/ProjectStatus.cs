// O namespace agrupa as classes de modelo, mantendo o código organizado.
namespace HumanResources.Models
{
    /// <summary>
    /// Define os possíveis estados de um Projeto.
    /// Uma enumeração é um tipo de valor que associa nomes a valores inteiros.
    /// Por defeito, os valores começam em 0 e são incrementados automaticamente.
    /// </summary>
    public enum ProjectStatus
    {
        /// <summary>
        /// O projeto foi planeado mas ainda não começou. (Valor numérico: 0)
        /// </summary>
        NotStarted,

        /// <summary>
        /// O projeto está atualmente em execução. (Valor numérico: 1)
        /// </summary>
        InProgress,

        /// <summary>
        /// O projeto foi finalizado com sucesso. (Valor numérico: 2)
        /// </summary>
        Completed,

        /// <summary>
        /// O projeto está temporariamente pausado. (Valor numérico: 3)
        /// </summary>
        OnHold,

        /// <summary>
        /// O projeto foi cancelado antes da sua conclusão. (Valor numérico: 4)
        /// </summary>
        Cancelled
    }
}