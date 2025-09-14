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
    // Controller MVC para CRUD de Contracts diretamente via DbContext (sem repositório).
    public class ContractsController : Controller
    {
        private readonly Context _context; // DbContext da aplicação

        public ContractsController(Context context)
        {
            _context = context; // injeção de dependência do DbContext
        }

        // GET: Contracts
        // Lista contratos incluindo navegações Client e Project (eager loading).
        public async Task<IActionResult> Index()
        {
            var context = _context.Contracts.Include(c => c.Client).Include(c => c.Project);
            return View(await context.ToListAsync());
        }

        // GET: Contracts/Details/5
        // Mostra detalhes de um contrato por id; 404 se não existir.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // id ausente
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.Project)
                .FirstOrDefaultAsync(m => m.Id == id); // busca único
            if (contract == null)
            {
                return NotFound(); // não encontrado
            }

            return View(contract);
        }

        // GET: Contracts/Create
        // Mostra formulário de criação; preenche dropdowns com Clients e Projects
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName");
            return View();
        }

        // POST: Contracts/Create
        // Protegido contra CSRF com [ValidateAntiForgeryToken]
        // O [Bind(...)] reduz risco de overposting limitando propriedades aceites do request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ServiceDescription,StartDate,ExpirationDate,Value,TermsAndConditions,ClientId,ProjectId")] Contract contract)
        {
            if (ModelState.IsValid) // validação por data annotations do modelo
            {
                _context.Add(contract);           // adiciona entidade ao contexto (estado Added)
                await _context.SaveChangesAsync(); // persiste na BD
                return RedirectToAction(nameof(Index)); // PRG pattern
            }
            // Se falhar validação, reconstroi dropdowns mantendo seleção atual
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", contract.ClientId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName", contract.ProjectId);
            return View(contract);
        }

        // GET: Contracts/Edit/5
        // Carrega contrato existente para edição; 404 se id inválido ou não existir
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id); // busca por PK (tracking)
            if (contract == null)
            {
                return NotFound();
            }
            // Preenche dropdowns com seleção do registo atual
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", contract.ClientId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName", contract.ProjectId);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        // Protegido contra CSRF. [Bind(...)] limita propriedades atualizáveis
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServiceDescription,StartDate,ExpirationDate,Value,TermsAndConditions,ClientId,ProjectId")] Contract contract)
        {
            if (id != contract.Id)
            {
                return NotFound(); // id do URL não corresponde ao do modelo
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contract);           // marca entidade como Modified
                    await _context.SaveChangesAsync();   // tenta gravar
                }
                catch (DbUpdateConcurrencyException)     // conflito de concorrência otimista
                {
                    if (!ContractExists(contract.Id))    // se já não existe, 404
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;                           // se for outro motivo, propaga exceção
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // Se validação falhar, reconstroi dropdowns e devolve a view com erros
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", contract.ClientId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName", contract.ProjectId);
            return View(contract);
        }

        // GET: Contracts/Delete/5
        // Mostra confirmação de eliminação; carrega também Client e Project para contexto
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contracts/Delete/5
        // Confirma eliminação; protegido contra CSRF
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract); // marca para remoção
            }

            await _context.SaveChangesAsync();       // executa DELETE na BD
            return RedirectToAction(nameof(Index));
        }

        // Helper para verificar existência de contrato por PK (usado na gestão de concorrência)
        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }
    }
}
