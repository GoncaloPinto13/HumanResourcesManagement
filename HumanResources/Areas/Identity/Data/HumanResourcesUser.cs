using HumanResources.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using HumanResources.Models; // Adicione este using para aceder ao modelo Client
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema; // Adicione este using para o ForeignKey


namespace HumanResources.Areas.Identity.Data;

// Add profile data for application users by adding properties to the HumanResourcesUser class
public class HumanResourcesUser : IdentityUser
{
    public int? ClientId { get; set; }

    // Propriedade de navegação para o Entity Framework Core
    [ForeignKey("ClientId")]
    public virtual Client Client { get; set; }
}

