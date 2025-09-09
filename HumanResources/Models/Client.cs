using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace HumanResources.Models
{
    [Table("Clients")]
    public class Client
    {
        [Key]
        [Column("ClientId")]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da empresa é obrigatório.")]
        [StringLength(100)]
        [Column("CompanyName")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "O NIF é obrigatório.")]
        [StringLength(9)]
        [Column("Nif")]
        public string Nif { get; set; }

        [EmailAddress]
        [StringLength(255)]
        [Column("Email")]
        public string Email { get; set; }

        // --- Relações (Lado "Um") ---

        // Relação 1-N: Um Cliente tem MUITOS Projetos
        public ICollection<Project> Projects { get; set; } = new List<Project>();

        // Relação 1-N: Um Cliente tem MUITOS Contratos
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
