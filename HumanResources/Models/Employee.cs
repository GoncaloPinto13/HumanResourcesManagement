using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResources.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        [Column("EmployeeId")]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do funcionário é obrigatório.")]
        [StringLength(100)]
        [Column("FullName")]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Position")]
        public string Position { get; set; }

        [StringLength(100)]
        [Column("SpecializationArea")]
        public string SpecializationArea { get; set; }

        // --- Relação de Navegação ---
        // Relação N-N (Muitos-para-Muitos) com Projetos
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
