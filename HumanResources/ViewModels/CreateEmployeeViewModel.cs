// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
// Importa a classe SelectList, que é um tipo de dados otimizado para criar listas de opções (dropdowns) em HTML.
using Microsoft.AspNetCore.Mvc.Rendering;
// Importa atributos para validação de dados.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

// O namespace agrupa os ViewModels, separando-os dos modelos de dados do domínio.
namespace HumanResources.ViewModels
{
    /// <summary>
    /// ViewModel para a view de criação de um novo Funcionário, incluindo a sua conta de utilizador e perfil.
    /// Encapsula todos os campos necessários do formulário num único objeto.
    /// </summary>
    public class CreateEmployeeViewModel
    {
        // --- Propriedades do Funcionário ---

        [Required(ErrorMessage = "O nome do funcionário é obrigatório.")]
        [Display(Name = "Nome Completo")] // Define o texto do 'label' na UI.
        public string Name { get; set; }

        [Required(ErrorMessage = "O cargo é obrigatório.")]
        [Display(Name = "Cargo")]
        public string Position { get; set; }

        // O comentário sugere que o ViewModel é extensível.
        // Se fossem necessários mais campos para o funcionário, poderiam ser adicionados aqui.
        // Ex: public string SpecializationArea { get; set; }

        // --- Propriedades da Conta de Login ---

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress] // Valida o formato do email.
        [Display(Name = "Email de Login")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A password é obrigatória.")]
        [DataType(DataType.Password)] // Ajuda a UI a renderizar um campo de password.
        [StringLength(100, ErrorMessage = "A {0} deve ter no mínimo {2} caracteres.", MinimumLength = 6)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        // --- Propriedade para a Seleção do Perfil ---

        /// <summary>
        /// Esta propriedade irá receber o nome do perfil (ex: "Employee", "Project Manager")
        /// que o utilizador selecionou na dropdown do formulário.
        /// </summary>
        [Required(ErrorMessage = "É obrigatório selecionar um perfil.")]
        [Display(Name = "Perfil de Acesso")]
        public string Role { get; set; }

        /// <summary>
        /// Esta propriedade é usada para ENVIAR a lista de perfis disponíveis do Controller PARA a View.
        /// A View utilizará este objeto `SelectList` para construir o HTML da dropdown.
        /// NOTA: É uma excelente prática de segurança adicionar o atributo [BindNever] a esta propriedade,
        /// para garantir que nenhum dado vindo do utilizador possa modificá-la num pedido POST.
        /// Ex: [BindNever] public SelectList RoleList { get; set; }
        /// </summary>
        
        [BindNever] // <-- ADICIONE ESTE ATRIBUTO
        [ValidateNever]
        public SelectList RoleList { get; set; }
        
    }
}