using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;                 // MVC base
using Microsoft.AspNetCore.Mvc.Rendering;       
using Microsoft.EntityFrameworkCore;            // EF Core (LINQ async, exceções)
using HumanResources.Data;
using HumanResources.Models;

namespace HumanResources.Controllers
{
    // Controller MVC para CRUD de Employees diretamente via DbContext (sem repositório).
    public class EmployeesController : Controller
    {
        private readonly Context _context; // DbContext da aplicação

        public EmployeesController(Context context)
        {
            _context = context; // injeção de dependência do DbContext
        }

        // GET: Employees
        // Lista todos os empregados
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employees.ToListAsync());
        }

        // GET: Employees/Details/5
        // Mostra detalhes de um empregado por id; 404 se id nulo ou não encontrado
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // id ausente
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id); // busca único
            if (employee == null)
            {
                return NotFound(); // não encontrado
            }

            return View(employee);
        }

        // GET: Employees/Create
        // Mostra formulário de criação
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create   
        // Proteção anti-CSRF com [ValidateAntiForgeryToken]
        // [Bind(...)] limita propriedades aceites, mitigando overposting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Position,SpecializationArea")] Employee employee)
        {
            if (ModelState.IsValid)                 // validação por data annotations do modelo
            {
                _context.Add(employee);             // adiciona entidade (estado Added)
                await _context.SaveChangesAsync();  // persiste na BD
                return RedirectToAction(nameof(Index)); // PRG pattern
            }
            return View(employee);                  // devolve com erros de validação
        }

        // GET: Employees/Edit/5
        // Carrega empregado existente para edição; 404 se id inválido ou não existir
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id); // busca por PK (tracking)
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        // Proteção anti-CSRF. [Bind(...)] limita propriedades atualizáveis
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Position,SpecializationArea")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound(); // id do URL difere do do modelo
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);         // marca como Modified
                    await _context.SaveChangesAsync(); // guarda alterações
                }
                catch (DbUpdateConcurrencyException)   // concorrência otimista (row apagada/alterada)
                {
                    if (!EmployeeExists(employee.Id))  // já não existe → 404
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;                         // outro problema de concorrência → propaga
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee); // volta à view com erros
        }

        // GET: Employees/Delete/5
        // Mostra confirmação de eliminação; 404 se não existir
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        // Confirma eliminação; protegido contra CSRF
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee); // marca para remoção
            }

            await _context.SaveChangesAsync();       // executa DELETE
            return RedirectToAction(nameof(Index));
        }

        // Helper para verificar existência por PK (usado na gestão de concorrência)
        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
