// Adicionam-se todos os 'usings' necessários no topo do ficheiro.
using HumanResources.Data;
using HumanResources.Models;
using HumanResources.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// --- 1. CONFIGURAÇÃO DOS SERVIÇOS ---

// Adiciona o DbContext para o Entity Framework
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(connectionString));

// Adiciona e configura o ASP.NET Core Identity
// - Usa a sua classe 'User'
// - Ativa o sistema de 'Roles' (Perfis)
// - Liga o Identity ao seu 'Context' da base de dados
builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<Context>();

builder.Services.AddControllersWithViews();


// --- 2. CONSTRUÇÃO DA APLICAÇÃO ---
var app = builder.Build();


// --- 3. EXECUÇÃO DO SEED SERVICE (FORMA SEGURA) ---
// Este bloco garante que os serviços são obtidos de forma correta durante o arranque.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Chama o seu método para popular a base de dados com perfis e o admin.
        await SeedService.SeedDatabase(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao popular a base de dados (seeding).");
    }
}


// --- 4. CONFIGURAÇÃO DO PIPELINE HTTP ---

// Configura o tratamento de erros para ambientes de produção.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Ativa a autenticação e autorização. A ordem é crucial.
app.UseAuthentication();
app.UseAuthorization();

// Mapeia a rota padrão para os seus controllers MVC.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapeia as páginas do Identity (Login, Registo, etc.).
app.MapRazorPages();

app.Run();

