using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace HumanResources.Models
{

    [Table("Projects")]
    public class Project
    {
        [Key]
        [Column("ProjectId")]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Column("ProjectName")]
        public string? ProjectName { get; set; }

        [Column("Description", TypeName = "nvarchar(MAX)")]
        public string? Description { get; set; }

        [Column("StartDate", TypeName = "date")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Column("DueDate", TypeName = "date")]
        public DateTime DueDate { get; set; }

        [Column("Budget", TypeName = "decimal(18, 2)")]
        public decimal Budget { get; set; }

        [Column("Status", TypeName = "nvarchar(MAX)")]
        public ProjectStatus ProjectStatus { get; set; }


        // --- Relações ---

        // Relação N-1: Um Projeto pertence a UM Cliente
        [Required]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        // Relação 1-1: Um Projeto tem varios Contratos
        // A chave estrangeira está definida na classe Contract
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

        // Relação N-N: Um Projeto tem MUITOS Funcionários
        //public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}