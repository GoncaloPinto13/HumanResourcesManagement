// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
// Importa a classe de utilizador personalizada do ASP.NET Core Identity.
using HumanResources.Areas.Identity.Data;
// Importa atributos para validação de dados, como [Key], [Required], [StringLength].
using System.ComponentModel.DataAnnotations;
// Importa atributos para mapeamento do esquema da base de dados, como [Table], [Column], [ForeignKey].
using System.ComponentModel.DataAnnotations.Schema;
// Importa funcionalidades para contratos de código (neste caso, não está a ser usado ativamente, mas foi importado).
using System.Diagnostics.Contracts;

// O namespace agrupa as classes de modelo, mantendo o código organizado.
namespace HumanResources.Models
{
    /// <summary>
    /// Representa a entidade Cliente no sistema.
    /// O atributo [Table("Clients")] especifica que esta classe deve ser mapeada
    /// para uma tabela na base de dados chamada "Clients", em vez do nome padrão "Client".
    /// </summary>
    [Table("Clients")]
    public class Client
    {
        // O atributo [Key] designa esta propriedade como a chave primária da tabela.
        [Key]
        // O atributo [Column("ClientId")] renomeia a coluna na base de dados para "ClientId".
        // Se omitido, o nome da coluna seria "Id" por convenção.
        public int Id { get; set; }

        // O atributo [Required] torna este campo obrigatório. Se um formulário for submetido
        // sem este valor, a validação do modelo falhará. A mensagem de erro é personalizável.
        [Required(ErrorMessage = "O nome da empresa é obrigatório.")]
        // Limita o tamanho máximo da string a 100 caracteres, tanto na aplicação como na base de dados.
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "O NIF é obrigatório.")]
        // Limita o NIF a 9 caracteres.
        [StringLength(9)]
        public string Nif { get; set; }

        // --- Relação com a Conta de Login (Identity) ---

        // Esta é a propriedade da CHAVE ESTRANGEIRA. Ela armazena o ID do utilizador (da tabela AspNetUsers)
        // que está associado a este cliente. É um `string` porque os IDs do ASP.NET Core Identity são strings.
        public string UserId { get; set; }

        // Esta é a PROPRIEDADE DE NAVEGAÇÃO. Permite aceder ao objeto `HumanResourcesUser` completo
        // a partir de um objeto `Client` (ex: `meuCliente.User.Email`).
        // O atributo [ForeignKey("UserId")] liga explicitamente esta propriedade de navegação
        // à chave estrangeira `UserId` definida acima.
        // A palavra-chave `virtual` permite que o Entity Framework use "lazy loading" (carregamento preguiçoso),
        // o que significa que os dados do utilizador só são carregados da BD quando são explicitamente acedidos.
        [ForeignKey("UserId")]
        public virtual HumanResourcesUser User { get; set; }

        // --- Relações (Lado "Um") ---

        // Esta é uma PROPRIEDADE DE NAVEGAÇÃO DE COLEÇÃO, representando o lado "muitos"
        // de uma relação um-para-muitos (1-N). Um cliente pode ter vários projetos.
        // A inicialização `= new List<Project>()` é uma boa prática para evitar erros de referência nula
        // quando se tenta adicionar um projeto a um cliente recém-criado.
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

        // Propriedade de navegação semelhante para a relação 1-N com Contratos.
        // Um cliente pode ter vários contratos.
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}