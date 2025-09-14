using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using HumanResources.Data;
using HumanResources.Models;
using HumanResources.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // para DbUpdateException

namespace HumanResources.Controllers
{
    // Controller MVC para CRUD de Clients usando um repositório (IClientRepository).
    // Não há lógica de acesso a dados aqui; tudo passa pelo _repo.
    public class ClientsController : Controller
    {
        // Repositório injetado via DI.
        private readonly IClientRepository _repo;
        public ClientsController(IClientRepository repo) => _repo = repo;

        // GET: /Clients
        // Lista todos os clientes ou filtra por nome da empresa (q).
        public async Task<IActionResult> Index(string? q)
        {
            var list = string.IsNullOrWhiteSpace(q)
                ? await _repo.GetAllAsync()                 // sem query: devolve todos
                : await _repo.SearchByCompanyAsync(q);      // com query: pesquisa por CompanyName
            return View(list);                              // devolve a lista à View
        }

        // GET: /Clients/Details/5
        // Mostra detalhes de um cliente específico.
        public async Task<IActionResult> Details(int id)
        {
            var client = await _repo.GetByIdAsync(id);     // procura pela PK
            if (client is null) return NotFound();         // erro se não existir
            return View(client);                           // mostra os detalhes
        }

        // GET: /Clients/Create
        // Mostra o formulário de criação.
        public IActionResult Create() => View();

        // POST: /Clients/Create
        // Cria um novo cliente. Protegido com AntiForgeryToken.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyName,Nif,Email")] Client client)
        {
            // Se o ModelState tiver erros de validação, devolve a view com os erros
            if (!ModelState.IsValid) return View(client);

            // Validação de unicidade do NIF a nível de aplicação
            if (await _repo.ExistsAsync(c => c.Nif == client.Nif))
            {
                ModelState.AddModelError(nameof(Client.Nif), "NIF já existe."); // mensagem direta ao campo
                return View(client);
            }

            await _repo.AddAsync(client);                  // persiste
            return RedirectToAction(nameof(Index));        // pós-redirect-get para evitar repost
        }

        // GET: /Clients/Edit/5
        // Mostra o formulário de edição com dados atuais.
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _repo.GetByIdAsync(id);
            if (client is null) return NotFound();
            return View(client);
        }

        // POST: /Clients/Edit/5
        // Atualiza um cliente existente. Protegido com AntiForgeryToken.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,Nif,Email")] Client client)
        {
            // Segurança: o id da rota tem de bater com o do modelo.
            if (id != client.Id) return BadRequest();
            if (!ModelState.IsValid) return View(client);

            // Validação de unicidade do NIF ao editar (exclui o próprio registo).
            var nifExiste = await _repo.ExistsAsync(c => c.Nif == client.Nif && c.Id != client.Id);
            if (nifExiste)
            {
                ModelState.AddModelError(nameof(Client.Nif), "NIF já existe.");
                return View(client);
            }

            await _repo.UpdateAsync(client);               // atualiza
            return RedirectToAction(nameof(Index));
        }

        // GET: /Clients/Delete/5
        // Mostra a página de confirmação de eliminação.
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _repo.GetByIdAsync(id);
            if (client is null) return NotFound();
            return View(client);
        }

        // POST: /Clients/Delete/5
        // Confirma e tenta eliminar o cliente.
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _repo.DeleteAsync(id);               // tenta apagar
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // Quando existem FKs/relacionamentos com DeleteBehavior.Restrict a impedir a remoção.
                ModelState.AddModelError("", "Não é possível apagar: existem registos relacionados.");
                var client = await _repo.GetByIdAsync(id); // recarrega para voltar à mesma view com dados
                return View("Delete", client);
            }
        }
    }
}
