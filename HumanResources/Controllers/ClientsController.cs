using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResources.Data;
using HumanResources.Models;
using HumanResources.ViewModels; // ADICIONADO: Para usar o ViewModel
using Microsoft.AspNetCore.Identity; // ADICIONADO: Para usar o UserManager
using HumanResources.Areas.Identity.Data; // ADICIONADO: Para a classe HumanResourcesUser

namespace HumanResources.Controllers
{
    public class ClientsController : Controller
    {
        private readonly HumanResourcesContext _context;
        private readonly UserManager<HumanResourcesUser> _userManager; // ADICIONADO

        // Construtor atualizado para injetar o UserManager
        public ClientsController(HumanResourcesContext context, UserManager<HumanResourcesUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Clients - Lógica atualizada para incluir a informação do User
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients.Include(c => c.User).ToListAsync();
            return View(clients);
        }

        // GET: Clients/Details/5 - Atualizado para int
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.User) // Inclui dados do user para mostrar na view de detalhes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // --- LÓGICA DE CRIAÇÃO TOTALMENTE REFEITA ---

        // GET: Clients/Create - Mostra o formulário para o ViewModel
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create - Recebe o ViewModel e cria o User + Client
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClientViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Criar o objeto User primeiro
                var user = new HumanResourcesUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // 2. Se o User foi criado, atribuir-lhe o perfil "Cliente"
                    await _userManager.AddToRoleAsync(user, "Cliente");

                    // 3. Criar o objeto Cliente e fazer a ligação
                    var client = new Client
                    {
                        // O Id é gerado automaticamente pela base de dados
                        CompanyName = model.CompanyName,
                        Nif = model.Nif,
                        UserId = user.Id // A "ponte" é feita aqui!
                    };

                    _context.Add(client);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                // Se a criação do User falhou, mostrar os erros no formulário
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // Se o modelo não for válido, retorna à view com os dados preenchidos
            return View(model);
        }

        // --- RESTANTES MÉTODOS ATUALIZADOS PARA INT ---

        // GET: Clients/Edit/5 - Atualizado para int
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,Nif,UserId")] Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Delete/5 - Atualizado para int
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5 - Atualizado para int
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}

