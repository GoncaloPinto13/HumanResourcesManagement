// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
// Importa o modelo Client para poder ser usado como propriedade de navegação.
using HumanResources.Models;
// Importa a classe base IdentityUser, da qual a nossa classe vai herdar.
using Microsoft.AspNetCore.Identity;
// Importa o atributo [ForeignKey] para configurar explicitamente a relação.
using System.ComponentModel.DataAnnotations.Schema;

// O namespace organiza as classes relacionadas com a identidade da aplicação.
namespace HumanResources.Areas.Identity.Data;

/// <summary>
/// Classe de utilizador personalizada para a aplicação. Herda de IdentityUser
/// e adiciona propriedades específicas do domínio, como a ligação a um Cliente.
/// </summary>
public class HumanResourcesUser : IdentityUser
{
    /// <summary>
    /// Chave estrangeira que liga o utilizador a um registo na tabela de Clientes.
    /// É um 'int?' (inteiro anulável) porque nem todos os utilizadores serão clientes
    /// (ex: Administradores, Funcionários). Apenas os utilizadores com o perfil "Cliente"
    /// terão esta propriedade preenchida. Para os outros, o valor será 'null'.
    /// </summary>
    public int? ClientId { get; set; }

    /// <summary>
    /// Propriedade de navegação que permite ao Entity Framework Core carregar o objeto
    /// 'Client' completo associado a este utilizador.
    /// O atributo [ForeignKey("ClientId")] especifica que esta propriedade de navegação
    /// está ligada à chave estrangeira 'ClientId' definida acima.
    /// A palavra-chave 'virtual' permite que o EF Core use "lazy loading" (carregamento preguiçoso).
    /// </summary>
    [ForeignKey("ClientId")]
    public virtual Client Client { get; set; }
}