// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
// Importa atributos para validação de dados e mapeamento do esquema da base de dados.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// O namespace agrupa as classes de modelo, mantendo o código organizado.
namespace HumanResources.Models
{
    /// <summary>
    // Uma enumeração (enum) define um conjunto de constantes nomeadas.
    // Usar um enum para o estado do contrato torna o código mais legível e seguro
    // do que usar números ou strings "mágicas" (ex: if (contract.Status == 0)).
    /// </summary>
    public enum ContractStatus
    {
        NotStarted,  // O contrato ainda não começou. (Valor numérico 0)
        InProgress,  // O contrato está ativo e em andamento. (Valor numérico 1)
        Completed,   // O contrato foi concluído. (Valor numérico 2)
        OnHold,      // O contrato está pausado. (Valor numérico 3)
        Cancelled    // O contrato foi cancelado. (Valor numérico 4)
    }

    /// <summary>
    /// Representa a entidade Contrato no sistema.
    /// O atributo [Table("Contracts")] mapeia esta classe para a tabela "Contracts" na base de dados.
    /// </summary>
    [Table("Contracts")]
    public class Contract
    {
        [Key]
        [Column("ContractId")]
        public int Id { get; set; }

        [Required] // Campo obrigatório.
        [StringLength(200)] // Tamanho máximo de 200 caracteres.
        [Column("ServiceDescription")] // Mapeia para a coluna "ServiceDescription".
        public string ServiceDescription { get; set; }

        // O atributo TypeName especifica o tipo de dados exato na base de dados.
        // "date" armazena apenas a data (ex: '2025-09-19'), sem a componente de tempo, otimizando o armazenamento.
        [Column("StartDate", TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Column("ExpirationDate", TypeName = "date")]
        public DateTime ExpirationDate { get; set; }

        // "decimal(18, 2)" define um número com um total de 18 dígitos,
        // sendo 2 deles para as casas decimais. Ideal para valores monetários.
        [Column("Value", TypeName = "decimal(18, 2)")]
        public decimal Value { get; set; }

        // ATENÇÃO: Existe aqui uma potencial inconsistência. A propriedade é do tipo 'bool' (verdadeiro/falso),
        // mas o tipo da coluna na BD está definido como 'nvarchar(MAX)', que é para textos muito longos.
        // Normalmente, um 'bool' seria mapeado para um tipo 'bit' na base de dados.
        // Isto pode ser um erro ou uma decisão de design específica que precisa de tratamento especial.
        [Column("TermsAndConditions", TypeName = "nvarchar(MAX)")]
        public bool TermsAndConditions { get; set; }

        [Column("RealValue", TypeName = "decimal(18, 2)")]
        public decimal RealValue { get; set; }

        [Column("Status")] // Mapeia para a coluna "Status".
        // O EF Core irá, por defeito, guardar o valor numérico do enum na base de dados.
        public ContractStatus Status { get; set; }

        [Column("IsOnStandby")] // Mapeia para a coluna "IsOnStandby".
        public bool IsOnStandby { get; set; }

        // --- Relações ---

        // Um contrato está sempre ligado a UM projeto.
        // Esta é a propriedade da CHAVE ESTRANGEIRA.
        [Required] // Garante que um contrato não pode ser criado sem um ProjectId.
        public int ProjectId { get; set; }
        // Esta é a PROPRIEDADE DE NAVEGAÇÃO. Permite aceder ao objeto Project
        // completo a partir de uma instância de Contract (ex: `meuContrato.Project.ProjectName`).
        public Project Project { get; set; }

        // Esta é uma PROPRIEDADE DE NAVEGAÇÃO DE COLEÇÃO que representa a relação Muitos-para-Muitos.
        // Um contrato pode ter muitos funcionários, e um funcionário pode estar em muitos contratos.
        // A ligação é feita através da tabela de junção `EmployeeContract`.
        // A partir de um contrato, podemos aceder a todas as suas associações com funcionários.
        public ICollection<EmployeeContract> EmployeeContracts { get; set; } = new List<EmployeeContract>();

        // --- Propriedades e Métodos Auxiliares ---

        // O atributo [NotMapped] diz ao Entity Framework para IGNORAR esta propriedade.
        // Ela não será convertida numa coluna na base de dados.
        // O seu valor é calculado dinamicamente pela aplicação sempre que é acedida.
        [NotMapped]
        public int DurationInDays => (int)(ExpirationDate - StartDate).TotalDays;

        /// <summary>
        /// Método auxiliar que verifica se o contrato ainda está dentro da sua data de validade.
        /// </summary>
        /// <returns>True se a data de expiração for hoje ou uma data futura; caso contrário, false.</returns>
        public bool IsActive()
        {
            return ExpirationDate >= DateTime.Now;
        }
    }
}