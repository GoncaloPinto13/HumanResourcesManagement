using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;                 // MVC base
using Microsoft.AspNetCore.Mvc.Rendering;       // SelectList para dropdowns
using Microsoft.EntityFrameworkCore;            // EF Core (Include, SaveChangesAsync, exceções)
using HumanResources.Data;
using HumanResources.Models;

namespace HumanResources.Controllers
{
    // Controller MVC para CRUD de Projects diretamente via DbContext (sem repositório).
    public class ProjectsController : Controller
    {
        private readonly Context _context; // DbContext da aplicação

        public ProjectsController(Context context)
        {
            _context = context; // injeção de dependência do DbContext
        }

        // GET: Projects
        // Lista projetos incluindo a navegação Client (eager loading).
        public async Task<IActionResult> Index()
        {
            var context = _context.Projects.Include(p => p.Client); // consulta com Include
            return View(await context.ToListAsync());               // executa e envia para a View
        }

        // GET: Projects/Details/5
        // Mostra detalhes de um projeto por id; 404 se não existir.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // id ausente
            }

            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(m => m.Id == id); // busca único
            if (project == null)
            {
                return NotFound(); // não encontrado
            }

            return View(project);
        }

        // GET: Projects/Create
        // Mostra formulário de criação; preenche dropdown de Clients.
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName");
            return View();
        }

        // POST: Projects/Create
        // Protegido contra CSRF com [ValidateAntiForgeryToken].
        // [Bind(...)] limita propriedades aceites do request (mitiga overposting).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectName,Description,StartDate,DueDate,Budget,ClientId")] Project project)
        {
            if (ModelState.IsValid) // validação via data annotations do modelo
            {
                _context.Add(project);              // adiciona entidade ao contexto
                await _context.SaveChangesAsync();  // persiste na BD
                return RedirectToAction(nameof(Index)); // PRG pattern
            }
            // Se falhar validação, reconstroi dropdown mantendo seleção atual
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", project.ClientId);
            return View(project);
        }

        // GET: Projects/Edit/5
        // Carrega projeto existente para edição; 404 se id inválido ou não existir.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id); // busca por PK (tracking)
            if (project == null)
            {
                return NotFound();
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", project.ClientId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // Protegido contra CSRF. [Bind(...)] limita propriedades atualizáveis.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectName,Description,StartDate,DueDate,Budget,ClientId")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound(); // id do URL não corresponde ao do modelo
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);          // marca entidade como Modified
                    await _context.SaveChangesAsync(); // tenta gravar na BD
                }
                catch (DbUpdateConcurrencyException)   // concorrência otimista (linha alterada/removida)
                {
                    if (!ProjectExists(project.Id))    // se já não existir → 404
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;                         // outro motivo → propaga exceção
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // Se validação falhar, reconstroi dropdown e devolve a view com erros
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", project.ClientId);
            return View(project);
        }

        // GET: Projects/Delete/5
        // Mostra confirmação de eliminação; carrega também Client para contexto.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        // Confirma eliminação; protegido contra CSRF.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project); // marca para remoção
            }

            await _context.SaveChangesAsync();      // executa DELETE
            return RedirectToAction(nameof(Index));
        }

        // Helper para verificar existência por PK (usado na gestão de concorrência).
        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
