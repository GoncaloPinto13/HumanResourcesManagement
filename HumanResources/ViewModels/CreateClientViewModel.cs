// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
// Importa atributos para validação de dados, como [Required], [EmailAddress], etc.
using System.ComponentModel.DataAnnotations;

// O namespace agrupa os ViewModels, separando-os dos modelos de dados do domínio.
namespace HumanResources.ViewModels
{
    /// <summary>
    /// ViewModel para a view de criação de um novo Cliente e da sua conta de utilizador associada.
    /// Combina campos das entidades 'Client' e 'HumanResourcesUser' para facilitar o model binding
    /// a partir de um único formulário.
    /// </summary>
    public class CreateClientViewModel
    {
        // --- Propriedades do Cliente ---

        [Required(ErrorMessage = "O nome da empresa é obrigatório.")] // Torna o campo obrigatório.
        [Display(Name = "Nome da Empresa")] // Define o texto do 'label' para este campo na UI.
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "O NIF é obrigatório.")]
        [StringLength(9)] // Valida que o NIF tem exatamente 9 caracteres (se não for especificado um `MinimumLength`).
        [Display(Name = "NIF")]
        public string Nif { get; set; }

        // --- Propriedades da Conta de Login ---

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress] // Valida se o formato do texto inserido corresponde a um endereço de email válido.
        [Display(Name = "Email de Login")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A password é obrigatória.")]
        // O atributo [DataType] é uma sugestão para a UI sobre como renderizar o campo.
        // `DataType.Password` faz com que seja gerado um <input type="password">,
        // que mascara os caracteres à medida que são digitados.
        [DataType(DataType.Password)]
        // Valida o comprimento da password, definindo um mínimo e um máximo.
        // A mensagem de erro pode usar placeholders: {0} para o nome do campo, {1} para o máx., {2} para o mín.
        [StringLength(100, ErrorMessage = "A {0} deve ter no mínimo {2} caracteres.", MinimumLength = 6)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}