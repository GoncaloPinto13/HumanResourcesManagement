using System.ComponentModel.DataAnnotations;

namespace HumanResources.ViewModels
{
    public class CreateClientViewModel
    {
        // --- Propriedades do Cliente ---
        [Required(ErrorMessage = "O nome da empresa é obrigatório.")]
        [Display(Name = "Nome da Empresa")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "O NIF é obrigatório.")]
        [StringLength(9)]
        [Display(Name = "NIF")]
        public string Nif { get; set; }

        // --- Propriedades da Conta de Login ---
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress]
        [Display(Name = "Email de Login")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A password é obrigatória.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "A {0} deve ter no mínimo {2} caracteres.", MinimumLength = 6)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
