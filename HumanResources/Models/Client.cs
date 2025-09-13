using HumanResources.Areas.Identity.Data;
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

        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "NIF is required.")]
        [StringLength(9)]
        public string Nif { get; set; }

        // --- Relação com a Conta de Login (Identity) ---

        // Chave Estrangeira que vai ligar ao Id da tabela AspNetUsers
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual HumanResourcesUser User { get; set; }

        // --- Relações (Lado "Um") ---

        // Relação 1-N: Um Cliente tem MUITOS Projetos
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

        // Relação 1-N: Um Cliente tem MUITOS Contratos
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
