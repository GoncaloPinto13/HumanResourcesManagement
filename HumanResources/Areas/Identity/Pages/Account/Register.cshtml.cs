// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
using HumanResources.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // Adicionado para usar a classe SelectList.
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

// O namespace indica a localização desta página dentro da estrutura de áreas do Identity.
namespace HumanResources.Areas.Identity.Pages.Account
{
    /// <summary>
    /// O atributo [AllowAnonymous] permite que utilizadores não autenticados acedam a esta página,
    /// o que é essencial para uma página de registo.
    /// </summary>
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        // --- INJEÇÃO DE DEPENDÊNCIAS ---
        // Serviços do ASP.NET Core Identity que são injetados no construtor.
        private readonly SignInManager<HumanResourcesUser> _signInManager;
        private readonly UserManager<HumanResourcesUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; // ADICIONADO: Para gerir os perfis.

        /// <summary>
        /// Construtor atualizado para receber o RoleManager, além dos gestores de utilizador e de login.
        /// </summary>
        public RegisterModel(
            UserManager<HumanResourcesUser> userManager,
            SignInManager<HumanResourcesUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// A propriedade 'Input' representa o modelo de dados para o formulário.
        /// O atributo [BindProperty] liga automaticamente os dados do formulário a esta propriedade
        /// quando um pedido POST é recebido.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Guarda o URL para o qual o utilizador será redirecionado após um registo bem-sucedido.
        /// </summary>
        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        /// ADICIONADO: Propriedade para fornecer a lista de perfis à página Razor (.cshtml),
        /// permitindo a construção de uma dropdown de seleção.
        /// </summary>
        public SelectList RoleList { get; set; }

        /// <summary>
        /// Classe interna que define a estrutura e as regras de validação para os campos do formulário.
        /// </summary>
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "A {0} deve ter no mínimo {2} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar password")]
            // O atributo [Compare] valida se o valor deste campo é igual ao do campo "Password".
            [Compare("Password", ErrorMessage = "A password e a sua confirmação não correspondem.")]
            public string ConfirmPassword { get; set; }

            /// <summary>
            /// ADICIONADO: Propriedade para receber o nome do perfil (Role) selecionado na dropdown do formulário.
            /// </summary>
            [Required]
            public string Role { get; set; }
        }

        /// <summary>
        /// Método executado quando a página é acedida através de um pedido GET.
        /// Prepara os dados necessários para renderizar o formulário.
        /// </summary>
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // ADICIONADO: Carrega a lista de perfis a partir da base de dados...
            // ...e cria um objeto SelectList, que é otimizado para ser usado em dropdowns HTML.
            // O primeiro "Name" é o valor (value) de cada opção e o segundo "Name" é o texto (text) visível.
            RoleList = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
        }

        /// <summary>
        /// Método executado quando o formulário é submetido (pedido POST).
        /// Processa os dados, cria o utilizador e o associa a um perfil.
        /// </summary>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/"); // Define a URL de retorno para a página inicial se não for especificada.
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Verifica se os dados do formulário cumprem todas as regras de validação definidas no InputModel.
            if (ModelState.IsValid)
            {
                // Cria uma nova instância do nosso utilizador personalizado.
                var user = new HumanResourcesUser { UserName = Input.Email, Email = Input.Email };
                // Tenta criar o utilizador na base de dados com a password fornecida.
                var result = await _userManager.CreateAsync(user, Input.Password);

                // Se a criação do utilizador foi bem-sucedida...
                if (result.Succeeded)
                {
                    // ADICIONADO: Associa o utilizador recém-criado ao perfil que foi selecionado no formulário.
                    if (!string.IsNullOrEmpty(Input.Role))
                    {
                        await _userManager.AddToRoleAsync(user, Input.Role);
                    }

                    // Efetua o login do novo utilizador na aplicação.
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    // Redireciona o utilizador para a URL de retorno.
                    return LocalRedirect(returnUrl);
                }
                // Se a criação falhou, adiciona os erros ao ModelState para serem exibidos na página.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Se o ModelState for inválido (a validação falhou), é necessário recarregar a lista de perfis
            // antes de renderizar a página novamente, para que a dropdown não apareça vazia.
            RoleList = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
            return Page();
        }
    }
}