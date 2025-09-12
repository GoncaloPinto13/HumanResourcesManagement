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
//    public class EmployeeContractsController : Controller
//    {
//        private readonly Context _context;

//        public EmployeeContractsController(Context context)
//        {
//            _context = context;
//        }

//        // GET: EmployeeContracts
//        public async Task<IActionResult> Index()
//        {
//            var context = _context.EmployeeContract.Include(e => e.Contract).Include(e => e.Employee);
//            return View(await context.ToListAsync());
//        }

//        // GET: EmployeeContracts/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var employeeContract = await _context.EmployeeContract
//                .Include(e => e.Contract)
//                .Include(e => e.Employee)
//                .FirstOrDefaultAsync(m => m.EmployeeContractId == id);
//            if (employeeContract == null)
//            {
//                return NotFound();
//            }

//            return View(employeeContract);
//        }

//        // GET: EmployeeContracts/Create
//        public IActionResult Create()
//        {
//            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceDescription");
//            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name");
//            return View();
//        }

//        // POST: EmployeeContracts/Create
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("EmployeeContractId,EmployeeId,ContractId,DurationInDays")] EmployeeContract employeeContract)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Add(employeeContract);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceDescription", employeeContract.ContractId);
//            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name", employeeContract.EmployeeId);
//            return View(employeeContract);
//        }

//        // GET: EmployeeContracts/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var employeeContract = await _context.EmployeeContract.FindAsync(id);
//            if (employeeContract == null)
//            {
//                return NotFound();
//            }
//            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceDescription", employeeContract.ContractId);
//            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name", employeeContract.EmployeeId);
//            return View(employeeContract);
//        }

//        // POST: EmployeeContracts/Edit/5
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("EmployeeContractId,EmployeeId,ContractId,DurationInDays")] EmployeeContract employeeContract)
//        {
//            if (id != employeeContract.EmployeeContractId)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    _context.Update(employeeContract);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!EmployeeContractExists(employeeContract.EmployeeContractId))
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
//            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceDescription", employeeContract.ContractId);
//            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name", employeeContract.EmployeeId);
//            return View(employeeContract);
//        }

//        // GET: EmployeeContracts/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var employeeContract = await _context.EmployeeContract
//                .Include(e => e.Contract)
//                .Include(e => e.Employee)
//                .FirstOrDefaultAsync(m => m.EmployeeContractId == id);
//            if (employeeContract == null)
//            {
//                return NotFound();
//            }

//            return View(employeeContract);
//        }

//        // POST: EmployeeContracts/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var employeeContract = await _context.EmployeeContract.FindAsync(id);
//            if (employeeContract != null)
//            {
//                _context.EmployeeContract.Remove(employeeContract);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool EmployeeContractExists(int id)
//        {
//            return _context.EmployeeContract.Any(e => e.EmployeeContractId == id);
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
    public class EmployeeContractsController : Controller
    {
        private readonly Context _context;

        public EmployeeContractsController(Context context)
        {
            _context = context;
        }

        // GET: EmployeeContracts
        public async Task<IActionResult> Index()
        {
            var context = _context.EmployeeContracts
                .Include(e => e.Employee)
                .Include(e => e.Contract);
            return View(await context.ToListAsync());
        }

        // GET: EmployeeContracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeContract = await _context.EmployeeContracts
                .Include(e => e.Employee)
                .Include(e => e.Contract)
                .FirstOrDefaultAsync(m => m.EmployeeContractId == id);

            if (employeeContract == null)
            {
                return NotFound();
            }

            return View(employeeContract);
        }

        // GET: EmployeeContracts/Create
        public IActionResult Create()
        {
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceDescription");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name");
            return View();
        }

        // POST: EmployeeContracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,ContractId,DurationInDays")] EmployeeContract employeeContract)
        {
            // O `ModelState.IsValid` pode falhar se o 'EmployeeContractId' (que é 0) for incluído na validação.
            // A solução é remover a validação para a chave primária, deixando o EF Core lidar com ela.
            
            ModelState.Remove("EmployeeContractId");
            ModelState.Remove("Employee");
            ModelState.Remove("Contract");
            if (ModelState.IsValid)
            {
                // Verifica se já existe uma ligação entre o funcionário e o contrato
                var existingLink = await _context.EmployeeContracts
                    .FirstOrDefaultAsync(ec => ec.EmployeeId == employeeContract.EmployeeId && ec.ContractId == employeeContract.ContractId);
               
                if (existingLink != null)
                {
                    ModelState.AddModelError("", "Esta ligação entre funcionário e contrato já existe.");
                }
                else
                {
                    _context.Add(employeeContract);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // Se o ModelState não for válido, recarrega as listas para a view
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceDescription", employeeContract.ContractId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name", employeeContract.EmployeeId);
            return View(employeeContract);
        }

        // GET: EmployeeContracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeContract = await _context.EmployeeContracts.FindAsync(id);
            if (employeeContract == null)
            {
                return NotFound();
            }
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceDescription", employeeContract.ContractId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name", employeeContract.EmployeeId);
            return View(employeeContract);
        }

        // POST: EmployeeContracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeContractId,EmployeeId,ContractId,DurationInDays")] EmployeeContract employeeContract)
        {
            if (id != employeeContract.EmployeeContractId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeContract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeContractExists(employeeContract.EmployeeContractId))
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
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceDescription", employeeContract.ContractId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name", employeeContract.EmployeeId);
            return View(employeeContract);
        }

        // GET: EmployeeContracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeContract = await _context.EmployeeContracts
                .Include(e => e.Employee)
                .Include(e => e.Contract)
                .FirstOrDefaultAsync(m => m.EmployeeContractId == id);
            if (employeeContract == null)
            {
                return NotFound();
            }

            return View(employeeContract);
        }

        // POST: EmployeeContracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employeeContract = await _context.EmployeeContracts.FindAsync(id);
            if (employeeContract != null)
            {
                _context.EmployeeContracts.Remove(employeeContract);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeContractExists(int id)
        {
            return _context.EmployeeContracts.Any(e => e.EmployeeContractId == id);
        }
    }
}
