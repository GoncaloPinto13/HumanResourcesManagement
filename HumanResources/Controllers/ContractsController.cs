using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HumanResources.Data;
using HumanResources.Models;
using HumanResources.ViewModels;


namespace HumanResources.Controllers
{
    public class ContractsController : Controller
    {
        private readonly HumanResourcesContext _context;

        public ContractsController(HumanResourcesContext context)
        {
            _context = context;
        }

        // GET: Contracts
        public async Task<IActionResult> Index()
        {
            var humanResourcesContext = _context.Contracts.Include(c => c.Project.Client);
            return View(await humanResourcesContext.ToListAsync());
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // ATUALIZE ESTA CONSULTA
            var contract = await _context.Contracts
                .Include(c => c.Project.Client)
                .Include(c => c.EmployeeContracts)  // <-- INCLUIR a tabela de junção
                    .ThenInclude(ec => ec.Employee) // <-- E DEPOIS INCLUIR o funcionário a partir da junção
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contracts/Create
        public async Task<IActionResult> Create()
        {
            // Vamos buscar apenas os funcionários que estão disponíveis.
            // Para isso, precisamos de carregar os seus contratos para usar a propriedade calculada "IsAvailable".
            var allEmployees = await _context.Employees
                                             .Include(e => e.EmployeeContracts)
                                             .ThenInclude(ec => ec.Contract)
                                             .ToListAsync();

            var availableEmployees = allEmployees.Where(e => e.IsAvailable);

            // Criamos o ViewModel
            var viewModel = new CreateContractViewModel
            {
                // Populamos a lista de seleção com os funcionários disponíveis
                AvailableEmployees = new SelectList(availableEmployees, "Id", "Name"),
                // Inicializamos um novo contrato para o formulário
                Contract = new Contract()
            };

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName");
            return View(viewModel);
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContractViewModel viewModel)
        {
            ModelState.Remove("AvailableEmployees");
            ModelState.Remove("Contract.Project");
            ModelState.Remove("Contract.EmployeeContracts");

            if (ModelState.IsValid)
            {
                // 1. Salvar o Contrato primeiro para obter um ID
                _context.Add(viewModel.Contract);
                await _context.SaveChangesAsync(); // Agora viewModel.Contract.Id tem o ID do novo contrato

                // 2. Associar os funcionários selecionados ao contrato recém-criado
                if (viewModel.SelectedEmployeeIds != null && viewModel.SelectedEmployeeIds.Any())
                {
                    foreach (var employeeId in viewModel.SelectedEmployeeIds)
                    {
                        var employeeContract = new EmployeeContract
                        {
                            EmployeeId = employeeId,
                            ContractId = viewModel.Contract.Id,
                            // Usamos a propriedade calculada do contrato para definir a duração
                            DurationInDays = viewModel.Contract.DurationInDays
                        };
                        _context.EmployeeContracts.Add(employeeContract);
                    }
                    // 3. Salvar as novas associações na tabela EmployeeContracts
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // Se o modelo for inválido, precisamos de repopular as listas antes de retornar à View
            var allEmployees = await _context.Employees
                                             .Include(e => e.EmployeeContracts)
                                             .ThenInclude(ec => ec.Contract)
                                             .ToListAsync();
            var availableEmployees = allEmployees.Where(e => e.IsAvailable);

            viewModel.AvailableEmployees = new SelectList(availableEmployees, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName", viewModel.Contract.ProjectId);

            return View(viewModel);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }
            //ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", contract.ClientId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName", contract.ProjectId);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServiceDescription,StartDate,ExpirationDate,Value,TermsAndConditions,ClientId,ProjectId")] Contract contract)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
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
           // ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", contract.ClientId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName", contract.ProjectId);
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
         .Include(c => c.Project.Client)
         .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }
    }
}