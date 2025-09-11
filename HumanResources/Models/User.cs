using Microsoft.AspNetCore.Identity; // ADICIONE ESTE USING!

namespace HumanResources.Models
{
    // A MUDANÇA PRINCIPAL ESTÁ AQUI: Adicionar ": IdentityUser"
    public class User : IdentityUser
    {
        // Coloque aqui APENAS as propriedades EXTRA que você quer adicionar.
        // O Id, UserName, Email, PasswordHash, etc., já vêm da classe IdentityUser.
        public string? FullName { get; set; }
    }
}