using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HumanResources.Data;
using HumanResources.Models;
using HumanResources.ViewModels; // ADICIONADO: Para usar o ViewModel
using Microsoft.AspNetCore.Identity; // ADICIONADO: Para usar os Managers
using HumanResources.Areas.Identity.Data; // ADICIONADO: Para a classe HumanResourcesUser

namespace HumanResources.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly HumanResourcesContext _context;
        private readonly UserManager<HumanResourcesUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // Construtor atualizado para injetar UserManager e RoleManager
        public EmployeesController(HumanResourcesContext context, UserManager<HumanResourcesUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            // Inclui os dados do User para podermos mostrar o email na lista
            var employees = await _context.Employees.Include(e => e.User).ToListAsync();
            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // --- LÓGICA DE CRIAÇÃO TOTALMENTE REFEITA ---

        // GET: Employees/Create
        public async Task<IActionResult> Create()
        {
            // Prepara o ViewModel com a lista de perfis disponíveis
            var roles = await _roleManager.Roles
                .Where(r => r.Name == "Employee" || r.Name == "Project Manager")
                .ToListAsync();

            var viewModel = new CreateEmployeeViewModel
            {
                RoleList = new SelectList(roles, "Name", "Name")
            };

            return View(viewModel);
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Criar o objeto User primeiro
                var user = new HumanResourcesUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // 2. Se o User foi criado, atribuir-lhe o perfil selecionado
                    await _userManager.AddToRoleAsync(user, model.Role);

                    // 3. Criar o objeto Employee e fazer a ligação
                    var employee = new Employee
                    {
                        Name = model.Name,
                        Position = model.Position,
                        UserId = user.Id // A "ponte" é feita aqui!
                    };

                    _context.Add(employee);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                // Se a criação do User falhou, mostrar os erros no formulário
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Se o modelo não for válido, recarregar a lista de perfis e mostrar o formulário novamente
            var roles = await _roleManager.Roles
                .Where(r => r.Name == "Employee" || r.Name == "Project Manager")
                .ToListAsync();
            model.RoleList = new SelectList(roles, "Name", "Name");

            return View(model);
        }

        // --- RESTANTES MÉTODOS MANTIDOS COM INT ---

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Position,SpecializationArea,UserId")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
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
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
