// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
// Importa atributos para validação de dados e mapeamento do esquema da base de dados.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Importa funcionalidades para contratos de código (neste caso, não está a ser usado ativamente).
using System.Diagnostics.Contracts;

// O namespace agrupa as classes de modelo.
namespace HumanResources.Models
{
    /// <summary>
    /// Representa a entidade Projeto no sistema.
    /// O atributo [Table("Projects")] mapeia esta classe para a tabela "Projects" na base de dados.
    /// </summary>
    [Table("Projects")]
    public class Project
    {
        [Key] // Define esta propriedade como a chave primária.
        [Column("ProjectId")] // Renomeia a coluna na base de dados para "ProjectId".
        public int Id { get; set; }

        [Required] // O nome do projeto é obrigatório.
        [StringLength(150)] // Limita o nome a um máximo de 150 caracteres.
        [Column("ProjectName")]
        // O `?` em `string?` indica que esta é uma "nullable reference type".
        // É uma funcionalidade do C# que ajuda a evitar erros de referência nula,
        // informando ao compilador que esta propriedade pode, intencionalmente, ser nula.
        public string? ProjectName { get; set; }

        // Mapeia para uma coluna do tipo `nvarchar(MAX)`, ideal para textos longos e sem limite definido.
        [Column("Description", TypeName = "nvarchar(MAX)")]
        public string? Description { get; set; }

        // Mapeia para uma coluna do tipo `date`, que armazena apenas a data, sem a hora.
        [Column("StartDate", TypeName = "date")]
        // A propriedade é inicializada com a data e hora atuais por defeito quando um novo objeto `Project` é criado.
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Column("DueDate", TypeName = "date")]
        public DateTime DueDate { get; set; }

        // Mapeia para um tipo `decimal` com precisão de 18 dígitos e 2 casas decimais, adequado para valores monetários.
        [Column("Budget", TypeName = "decimal(18, 2)")]
        public decimal Budget { get; set; }

        // NOTA: Esta configuração é específica. Por defeito, o EF Core guarda enums como números inteiros.
        // Ao usar `TypeName = "nvarchar(MAX)"` (ou "nvarchar(50)"), força-se o EF Core
        // a guardar o NOME do estado do enum (ex: "InProgress") como uma string na base de dados,
        // o que pode tornar os dados mais legíveis diretamente na BD.
        [Column("Status")] // O nome da coluna será "Status" por convenção.
        public ProjectStatus ProjectStatus { get; set; }


        // --- Relações ---

        // Relação Muitos-para-Um (N-1): Vários projetos podem pertencer ao mesmo cliente.
        [Required] // Um projeto DEVE ter um cliente associado.
        public int ClientId { get; set; } // Propriedade da Chave Estrangeira.
        public Client? Client { get; set; } // Propriedade de Navegação para aceder ao objeto Cliente.

        // Relação Um-para-Muitos (1-N): Um projeto pode ter vários contratos.
        // O comentário "1-1" está incorreto; uma `ICollection` representa o lado "muitos" de uma relação.
        // A chave estrangeira `ProjectId` está definida na classe `Contract`.
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

        // Relação Muitos-para-Muitos (N-N) - COMENTADA.
        // Se esta linha fosse descomentada, o EF Core criaria uma relação direta N-N
        // entre Projetos e Funcionários, gerando uma tabela de junção automática
        // na base de dados (ex: "EmployeeProject").
        // No modelo atual, a ligação entre funcionários e projetos é feita de forma indireta
        // através dos contratos (`Employee -> EmployeeContract -> Contract -> Project`).
        // public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}