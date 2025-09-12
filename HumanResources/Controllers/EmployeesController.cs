

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using HumanResources.Data;
//using HumanResources.Models;

//namespace HumanResources.Controllers
//{
//    public class EmployeesController : Controller
//    {
//        private readonly Context _context;

//        public EmployeesController(Context context)
//        {
//            _context = context;
//        }

//        // GET: Employees - Melhorada com filtros e pesquisa
//        public async Task<IActionResult> Index(string searchString, string specializationFilter, string sortOrder)
//        {
//            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
//            ViewData["PositionSortParm"] = sortOrder == "Position" ? "position_desc" : "Position";
//            ViewData["CurrentFilter"] = searchString;
//            ViewData["SpecializationFilter"] = specializationFilter;

//            var employees = from e in _context.Employees.Include(e => e.Contracts)
//                            select e;

//            // Pesquisa por nome ou posição
//            if (!String.IsNullOrEmpty(searchString))
//            {
//                employees = employees.Where(e => e.Name.Contains(searchString) ||
//                                               e.Position.Contains(searchString));
//            }

//            // Filtro por área de especialização
//            if (!String.IsNullOrEmpty(specializationFilter))
//            {
//                employees = employees.Where(e => e.SpecializationArea.Contains(specializationFilter));
//            }

//            // Ordenação
//            switch (sortOrder)
//            {
//                case "name_desc":
//                    employees = employees.OrderByDescending(e => e.Name);
//                    break;
//                case "Position":
//                    employees = employees.OrderBy(e => e.Position);
//                    break;
//                case "position_desc":
//                    employees = employees.OrderByDescending(e => e.Position);
//                    break;
//                default:
//                    employees = employees.OrderBy(e => e.Name);
//                    break;
//            }

//            return View(await employees.AsNoTracking().ToListAsync());
//        }

//        // GET: Employees/Details/5 - Melhorada com contratos
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var employee = await _context.Employees
//                .Include(e => e.Contracts)
//                    .ThenInclude(c => c.Client)
//                .Include(e => e.Contracts)
//                    .ThenInclude(c => c.Project)
//                .FirstOrDefaultAsync(m => m.Id == id);

//            if (employee == null)
//            {
//                return NotFound();
//            }

//            return View(employee);
//        }

//        // GET: Employees/Create
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // POST: Employees/Create - Melhorada com mensagens
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("Id,Name,Position,SpecializationArea")] Employee employee)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Add(employee);
//                await _context.SaveChangesAsync();
//                TempData["SuccessMessage"] = $"Funcionário {employee.Name} criado com sucesso!";
//                return RedirectToAction(nameof(Index));
//            }
//            return View(employee);
//        }

//        // GET: Employees/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var employee = await _context.Employees.FindAsync(id);
//            if (employee == null)
//            {
//                return NotFound();
//            }
//            return View(employee);
//        }

//        // POST: Employees/Edit/5 - Melhorada com mensagens
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Position,SpecializationArea")] Employee employee)
//        {
//            if (id != employee.Id)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    _context.Update(employee);
//                    await _context.SaveChangesAsync();
//                    TempData["SuccessMessage"] = $"Funcionário {employee.Name} atualizado com sucesso!";
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!EmployeeExists(employee.Id))
//                    {
//                        return NotFound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return RedirectToAction(nameof(Index));
//            }
//            return View(employee);
//        }

//        // GET: Employees/Delete/5 - Melhorada com contratos
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var employee = await _context.Employees
//                .Include(e => e.Contracts)
//                .FirstOrDefaultAsync(m => m.Id == id);

//            if (employee == null)
//            {
//                return NotFound();
//            }

//            return View(employee);
//        }

//        // POST: Employees/Delete/5 - Melhorada com verificação
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var employee = await _context.Employees
//                .Include(e => e.Contracts)
//                .FirstOrDefaultAsync(e => e.Id == id);

//            if (employee != null)
//            {
//                // Verificar se tem contratos associados
//                if (employee.Contracts.Any())
//                {
//                    TempData["ErrorMessage"] = $"Não é possível eliminar o funcionário {employee.Name} porque está associado a {employee.Contracts.Count} contrato(s).";
//                    return RedirectToAction(nameof(Index));
//                }

//                _context.Employees.Remove(employee);
//                await _context.SaveChangesAsync();
//                TempData["SuccessMessage"] = $"Funcionário {employee.Name} eliminado com sucesso!";
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        // NOVA: Gestão de Contratos do Funcionário
//        public async Task<IActionResult> ManageContracts(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var employee = await _context.Employees
//                .Include(e => e.Contracts)
//                .FirstOrDefaultAsync(e => e.Id == id);

//            if (employee == null)
//            {
//                return NotFound();
//            }

//            // Contratos disponíveis (não associados a este funcionário)
//            var availableContracts = await _context.Contracts
//                .Where(c => !employee.Contracts.Any(ec => ec.Id == c.Id))
//                .ToListAsync();

//            ViewBag.AvailableContracts = new SelectList(availableContracts, "Id", "ServiceDescription");

//            return View(employee);
//        }

//        // NOVA: Atribuir funcionário a contrato
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> AddToContract(int employeeId, int contractId)
//        {
//            var employee = await _context.Employees
//                .Include(e => e.Contracts)
//                .FirstOrDefaultAsync(e => e.Id == employeeId);

//            var contract = await _context.Contracts.FindAsync(contractId);

//            if (employee != null && contract != null)
//            {
//                if (!employee.Contracts.Any(c => c.Id == contractId))
//                {
//                    employee.Contracts.Add(contract);
//                    await _context.SaveChangesAsync();
//                    TempData["SuccessMessage"] = $"Funcionário {employee.Name} adicionado ao contrato {contract.ServiceDescription}!";
//                }
//                else
//                {
//                    TempData["ErrorMessage"] = "Funcionário já está atribuído a este contrato.";
//                }
//            }

//            return RedirectToAction(nameof(ManageContracts), new { id = employeeId });
//        }

//        // NOVA: Remover funcionário de contrato
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> RemoveFromContract(int employeeId, int contractId)
//        {
//            var employee = await _context.Employees
//                .Include(e => e.Contracts)
//                .FirstOrDefaultAsync(e => e.Id == employeeId);

//            if (employee != null)
//            {
//                var contract = employee.Contracts.FirstOrDefault(c => c.Id == contractId);
//                if (contract != null)
//                {
//                    employee.Contracts.Remove(contract);
//                    await _context.SaveChangesAsync();
//                    TempData["SuccessMessage"] = $"Funcionário removido do contrato!";
//                }
//            }

//            return RedirectToAction(nameof(ManageContracts), new { id = employeeId });
//        }

//        // NOVA: Relatórios e Estatísticas
//        public async Task<IActionResult> Reports()
//        {
//            var employeesWithContracts = await _context.Employees
//                .Include(e => e.Contracts)
//                .Select(e => new
//                {
//                    Employee = e,
//                    ContractCount = e.Contracts.Count,
//                    ActiveContractCount = e.Contracts.Count(c => c.ExpirationDate >= DateTime.Now)
//                })
//                .OrderByDescending(x => x.ContractCount)
//                .ToListAsync();

//            return View(employeesWithContracts);
//        }

//        private bool EmployeeExists(int id)
//        {
//            return _context.Employees.Any(e => e.Id == id);
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HumanResources.Data;
using HumanResources.Models;

namespace HumanResources.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly Context _context;

        public EmployeesController(Context context)
        {
            _context = context;
        }

        // GET: Employees - Melhorada com filtros e pesquisa
        public async Task<IActionResult> Index(string searchString, string specializationFilter, string sortOrder)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PositionSortParm"] = sortOrder == "Position" ? "position_desc" : "Position";
            ViewData["CurrentFilter"] = searchString;
            ViewData["SpecializationFilter"] = specializationFilter;

            var employees = from e in _context.Employees.Include(e => e.EmployeeContracts).ThenInclude(ec => ec.Contract)
                            select e;

            // Pesquisa por nome ou posição
            if (!String.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e => e.Name.Contains(searchString) ||
                                               e.Position.Contains(searchString));
            }

            // Filtro por área de especialização
            if (!String.IsNullOrEmpty(specializationFilter))
            {
                employees = employees.Where(e => e.SpecializationArea.Contains(specializationFilter));
            }

            // Ordenação
            switch (sortOrder)
            {
                case "name_desc":
                    employees = employees.OrderByDescending(e => e.Name);
                    break;
                case "Position":
                    employees = employees.OrderBy(e => e.Position);
                    break;
                case "position_desc":
                    employees = employees.OrderByDescending(e => e.Position);
                    break;
                default:
                    employees = employees.OrderBy(e => e.Name);
                    break;
            }

            return View(await employees.AsNoTracking().ToListAsync());
        }

        // GET: Employees/Details/5 - Melhorada com contratos
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.EmployeeContracts)
                    .ThenInclude(ec => ec.Contract)
                        .ThenInclude(c => c.Client)
                .Include(e => e.EmployeeContracts)
                    .ThenInclude(ec => ec.Contract)
                        .ThenInclude(c => c.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create - Melhorada com mensagens
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Position,SpecializationArea")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Funcionário {employee.Name} criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

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

        // POST: Employees/Edit/5 - Melhorada com mensagens
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Position,SpecializationArea")] Employee employee)
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
                    TempData["SuccessMessage"] = $"Funcionário {employee.Name} atualizado com sucesso!";
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

        // GET: Employees/Delete/5 - Melhorada com contratos
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.EmployeeContracts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5 - Melhorada com verificação
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.EmployeeContracts)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee != null)
            {
                // Verificar se tem contratos associados
                if (employee.EmployeeContracts.Any())
                {
                    TempData["ErrorMessage"] = $"Não é possível eliminar o funcionário {employee.Name} porque está associado a {employee.EmployeeContracts.Count} contrato(s).";
                    return RedirectToAction(nameof(Index));
                }

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Funcionário {employee.Name} eliminado com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Relatórios e Estatísticas
        public async Task<IActionResult> Reports()
        {
            var employeesWithContracts = await _context.Employees
                .Include(e => e.EmployeeContracts)
                .Select(e => new
                {
                    Employee = e,
                    ContractCount = e.EmployeeContracts.Count,
                    ActiveContractCount = e.EmployeeContracts.Count(ec => ec.Contract.ExpirationDate >= DateTime.Now)
                })
                .OrderByDescending(x => x.ContractCount)
                .ToListAsync();

            return View(employeesWithContracts);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}