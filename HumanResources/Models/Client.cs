using System.ComponentModel.DataAnnotations;         // DataAnnotations: validação de propriedades (Required, StringLength, EmailAddress)
using System.ComponentModel.DataAnnotations.Schema;  // Mapeamento para tabela/colunas (Table, Column)
using System.Diagnostics.Contracts;                  // Provavelmente desnecessário aqui; pode causar confusão com o modelo Contract

namespace HumanResources.Models
{
    [Table("Clients")] // Mapeia a entidade para a tabela "Clients"
    public class Client
    {
        [Key]                       // PK
        [Column("ClientId")]        // Nome da coluna na BD
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da empresa é obrigatório.")] // validação obrigatória
        [StringLength(100)]                                           // tamanho máximo 100
        [Column("CompanyName")]                                       // coluna "CompanyName"
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "O NIF é obrigatório.")] // obrigatório
        [StringLength(9)]                                 // limite de 9 chars (não garante só dígitos nem exatamente 9)
        [Column("Nif")]                                   // coluna "Nif"
        public string Nif { get; set; }
        
        // [RegularExpression(@"^\d{9}$", ErrorMessage = "NIF deve ter 9 dígitos.")]
        // E garante unicidade no nível da BD (índice único) — validação de app não chega

        [EmailAddress]             // valida formato de email (opcional porque não tem [Required])
        [StringLength(255)]        // tamanho máximo 255
        [Column("Email")]          // coluna "Email"
        public string Email { get; set; }

        // --- Relações (Lado "Um") ---

        // 1:N — Um Client tem muitos Projects
        // Inicializado para evitar null reference
        public ICollection<Project> Projects { get; set; } = new List<Project>();

        // 1:N — Um Client tem muitos Contracts
        // aqui usa-se o modelo HumanResources.Models.Contract (sem conflitos porque estamos no mesmo namespace)
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
