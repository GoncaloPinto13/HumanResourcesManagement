using System.ComponentModel.DataAnnotations;         // DataAnnotations: validação (Required, StringLength, etc.)
using System.ComponentModel.DataAnnotations.Schema;  // Mapeamento para BD (Table, Column)

namespace HumanResources.Models
{
    [Table("Employees")] // Mapeia a entidade para a tabela "Employees"
    public class Employee
    {
        [Key]                       // Chave primária
        [Column("EmployeeId")]      // Nome da coluna na BD
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do funcionário é obrigatório.")] // Obrigatório (mensagem personalizada)
        [StringLength(100)]                                             // Máx. 100 caracteres
        [Column("FullName")]                                            // Coluna "FullName"
        public string Name { get; set; }

        [Required]                 // Obrigatório
        [StringLength(50)]         // Máx. 50 caracteres
        [Column("Position")]       // Coluna "Position"
        public string Position { get; set; }

        [StringLength(100)]        // Opcional; máx. 100
        [Column("SpecializationArea")]
        public string SpecializationArea { get; set; }

      
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
