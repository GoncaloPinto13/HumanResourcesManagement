// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
// Importa atributos para validação de dados e mapeamento do esquema da base de dados.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// O namespace agrupa as classes de modelo, mantendo o código organizado.
namespace HumanResources.Models
{
    /// <summary>
    /// Representa a entidade de junção que liga um Funcionário (Employee) a um Contrato (Contract).
    /// Esta classe é essencial para modelar a relação Muitos-para-Muitos.
    /// O atributo [Table("EmployeeContracts")] mapeia a classe para a tabela de junção na base de dados.
    /// </summary>
    [Table("EmployeeContracts")]
    public class EmployeeContract
    {
        // O atributo [Key] define esta propriedade como a chave primária da tabela de junção.
        // Cada registo de associação (um funcionário específico num contrato específico) terá um ID único.
        [Key]
        public int EmployeeContractId { get; set; }

        // --- Chaves Estrangeiras ---
        // Estas duas propriedades formam a ligação fundamental da relação N-N.

        // Chave estrangeira que referencia o ID da tabela "Employees".
        public int EmployeeId { get; set; }
        // Chave estrangeira que referencia o ID da tabela "Contracts".
        public int ContractId { get; set; }

        // --- Propriedades da Relação (Payload) ---
        // Estas são as informações que pertencem à PRÓPRIA RELAÇÃO, e não apenas
        // ao funcionário ou ao contrato individualmente.

        [Display(Name = "Duração (dias)")] // Personaliza o nome do campo na UI.
        [Required(ErrorMessage = "A duração é obrigatória.")] // Validação para garantir que o campo é preenchido.
        public int DurationInDays { get; set; }

        [Display(Name = "Data de Início")]
        [Required(ErrorMessage = "A data de início é obrigatória.")]
        // O atributo [DataType] ajuda a interface do utilizador a renderizar o controlo correto
        // (neste caso, um seletor de data sem a parte do tempo).
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        // --- Propriedades de Navegação ---
        // Estas propriedades permitem aceder aos objetos completos a partir de um registo de EmployeeContract.
        // Por exemplo, a partir de `umEmployeeContract`, pode aceder a `umEmployeeContract.Employee.Name`.
        // NOTA: É comum declará-las como `virtual` para permitir o "lazy loading" do Entity Framework.

        public Employee Employee { get; set; }
        public Contract Contract { get; set; }
    }
}