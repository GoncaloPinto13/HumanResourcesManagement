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
    public class ClientsController : Controller
    {
        private readonly IClientRepository _repo;
        public ClientsController(IClientRepository repo) => _repo = repo;

        // GET: /Clients
        public async Task<IActionResult> Index(string? q)
        {
            var list = string.IsNullOrWhiteSpace(q)
                ? await _repo.GetAllAsync()
                : await _repo.SearchByCompanyAsync(q);
            return View(list);
        }

        // GET: /Clients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = await _repo.GetByIdAsync(id);
            if (client is null) return NotFound();
            return View(client);
        }

        // GET: /Clients/Create
        public IActionResult Create() => View();

        // POST: /Clients/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyName,Nif,Email")] Client client)
        {
            if (!ModelState.IsValid) return View(client);

            if (await _repo.ExistsAsync(c => c.Nif == client.Nif))
            {
                ModelState.AddModelError(nameof(Client.Nif), "NIF já existe.");
                return View(client);
            }

            await _repo.AddAsync(client);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Clients/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _repo.GetByIdAsync(id);
            if (client is null) return NotFound();
            return View(client);
        }

        // POST: /Clients/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,Nif,Email")] Client client)
        {
            if (id != client.Id) return BadRequest();
            if (!ModelState.IsValid) return View(client);

            // opcional: bloqueia editar para NIF duplicado
            var nifExiste = await _repo.ExistsAsync(c => c.Nif == client.Nif && c.Id != client.Id);
            if (nifExiste)
            {
                ModelState.AddModelError(nameof(Client.Nif), "NIF já existe.");
                return View(client);
            }

            await _repo.UpdateAsync(client);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Clients/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _repo.GetByIdAsync(id);
            if (client is null) return NotFound();
            return View(client);
        }

        // POST: /Clients/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _repo.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // DeleteBehavior.Restrict ou FKs a bloquear eliminação
                ModelState.AddModelError("", "Não é possível apagar: existem registos relacionados.");
                var client = await _repo.GetByIdAsync(id);
                return View("Delete", client);
            }
        }
    }
}

