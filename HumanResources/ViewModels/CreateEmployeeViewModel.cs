using Microsoft.AspNetCore.Mvc.Rendering; // Necessário para a SelectList
using System.ComponentModel.DataAnnotations;

namespace HumanResources.ViewModels
{
    public class CreateEmployeeViewModel
    {
        // --- Propriedades do Funcionário ---
        [Required(ErrorMessage = "O nome do funcionário é obrigatório.")]
        [Display(Name = "Nome Completo")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O cargo é obrigatório.")]
        [Display(Name = "Cargo")]
        public string Position { get; set; }

        // Adicione aqui outras propriedades do funcionário se necessário
        // Ex: public string SpecializationArea { get; set; }

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

        // --- Propriedade para a Seleção do Perfil ---
        [Required(ErrorMessage = "É obrigatório selecionar um perfil.")]
        [Display(Name = "Perfil de Acesso")]
        public string Role { get; set; }

        // Propriedade para popular a dropdown na View
        public SelectList RoleList { get; set; }
    }
}
