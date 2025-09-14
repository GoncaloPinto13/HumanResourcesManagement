using System.ComponentModel.DataAnnotations;         // DataAnnotations: validação (Required, StringLength, etc.)
using System.ComponentModel.DataAnnotations.Schema;  // Mapeamento para BD (Table, Column)
using System.Diagnostics.Contracts;                  

namespace HumanResources.Models
{

    [Table("Projects")] // Mapeia a entidade para a tabela "Projects"
    public class Project
    {
        [Key]                       // Chave primária
        [Column("ProjectId")]       // Nome da coluna na BD
        public int Id { get; set; }

        [Required]                  // Obrigatório
        [StringLength(150)]         // Máx. 150 caracteres
        [Column("ProjectName")]
        public string ProjectName { get; set; }

        [Column("Description", TypeName = "nvarchar(MAX)")] // Texto longo; opcional (sem [Required])
        public string Description { get; set; }

        [Column("StartDate", TypeName = "date")]  // Armazena só a data (sem hora) no SQL
        public DateTime StartDate { get; set; }

        [Column("DueDate", TypeName = "date")]    // Data de conclusão prevista (sem hora)
        public DateTime DueDate { get; set; }

        [Column("Budget", TypeName = "decimal(18, 2)")] // Valor monetário com 2 casas decimais
        public decimal Budget { get; set; }

        // --- Relações ---

        // Relação N-1: Um Projeto pertence a UM Cliente (muitos projetos podem referir o mesmo cliente)
        [Required]                 // FK obrigatória
        public int ClientId { get; set; }
        public Client Client { get; set; } // Navegação para o cliente

        
        public Contract Contract { get; set; }

        // Relação N-N: Um Projeto tem MUITOS Funcionários
        // EF Core cria automaticamente a tabela de junção (sem payload) com esta coleção.
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
