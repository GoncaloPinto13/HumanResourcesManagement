// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
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
    /// <summary>
    /// Controlador para gerir diretamente a entidade de junção EmployeeContract,
    /// que representa a alocação de um funcionário a um contrato.
    /// </summary>
    public class EmployeeContractsController : Controller
    {
        private readonly HumanResourcesContext _context;

        public EmployeeContractsController(HumanResourcesContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ação GET para a rota /EmployeeContracts.
        /// Lista todas as associações existentes entre funcionários e contratos.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // A consulta carrega cada 'EmployeeContract' e inclui os dados
            // do Funcionário (Employee) e do Contrato (Contract) associados.
            // Isto é necessário para exibir informações úteis, como os nomes, na tabela da view.
            var context = _context.EmployeeContracts
                .Include(e => e.Employee)
                .Include(e => e.Contract);
            return View(await context.ToListAsync());
        }

        /// <summary>
        /// Ação GET para a rota /EmployeeContracts/Details/{id}.
        /// Mostra os detalhes de uma associação específica.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employeeContract = await _context.EmployeeContracts
                .Include(e => e.Contract)
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(m => m.EmployeeContractId == id);

            if (employeeContract == null) return NotFound();

            return View(employeeContract);
        }

        /// <summary>
        /// Ação GET para a rota /EmployeeContracts/Create.
        /// Apresenta o formulário para criar uma nova associação manual entre um funcionário e um contrato.
        /// </summary>
        public IActionResult Create()
        {
            // Popula as dropdowns na view com todos os contratos e funcionários disponíveis
            // para que o utilizador possa selecionar.
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceDescription");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name");
            return View();
        }

        /// <summary>
        /// Ação POST para a rota /EmployeeContracts/Create.
        /// Processa os dados do formulário para criar uma nova associação, prevenindo entradas duplicadas.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeContractId,EmployeeId,ContractId,DurationInDays,StartDate")] EmployeeContract employeeContract)
        {
            // Manipulação do ModelState:
            // Removemos manualmente chaves que não são diretamente submetidas pelo utilizador
            // ou que poderiam causar falhas de validação.
            // 'EmployeeContractId' é gerado pela BD. 'Employee' e 'Contract' são propriedades de navegação.
            ModelState.Remove("EmployeeContractId");
            ModelState.Remove("Employee");
            ModelState.Remove("Contract");

            if (ModelState.IsValid)
            {
                // Lógica de negócio: Verificar se esta associação exata já existe.
                var existingLink = await _context.EmployeeContracts
                    .FirstOrDefaultAsync(ec => ec.EmployeeId == employeeContract.EmployeeId && ec.ContractId == employeeContract.ContractId);

                if (existingLink != null)
                {
                    // Se a ligação já existe, adiciona um erro ao modelo e retorna à view.
                    ModelState.AddModelError(string.Empty, "Esta ligação entre funcionário e contrato já existe.");
                }
                else
                {
                    // Se não houver duplicados, adiciona o novo registo e guarda na BD.
                    _context.Add(employeeContract);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // Se o ModelState for inválido (ou se houve um erro de duplicado),
            // é necessário repopular as dropdowns antes de mostrar o formulário novamente.
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceDescription", employeeContract.ContractId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name", employeeContract.EmployeeId);
            return View(employeeContract);
        }

        /// <summary>
        /// Ação GET para a rota /EmployeeContracts/Edit/{id}.
        /// Apresenta o formulário para editar uma associação existente.
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employeeContract = await _context.EmployeeContracts.FindAsync(id);

            if (employeeContract == null) return NotFound();

            // Popula as dropdowns, pré-selecionando os valores atuais da associação.
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceDescription", employeeContract.ContractId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name", employeeContract.EmployeeId);
            return View(employeeContract);
        }

        /// <summary>
        /// Ação POST para a rota /EmployeeContracts/Edit/{id}.
        /// Processa os dados do formulário para atualizar a associação.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeContractId,EmployeeId,ContractId,DurationInDays,StartDate")] EmployeeContract employeeContract)
        {
            if (id != employeeContract.EmployeeContractId) return NotFound();

            // É necessário remover novamente as propriedades de navegação para evitar problemas
            // de validação ao atualizar o modelo.
            ModelState.Remove("Employee");
            ModelState.Remove("Contract");

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

        /// <summary>
        /// Ação GET para a rota /EmployeeContracts/Delete/{id}.
        /// Apresenta a página de confirmação para apagar uma associação.
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employeeContract = await _context.EmployeeContracts
                .Include(e => e.Contract)
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(m => m.EmployeeContractId == id);

            if (employeeContract == null) return NotFound();

            return View(employeeContract);
        }

        /// <summary>
        /// Ação POST para a rota /EmployeeContracts/Delete/{id}.
        /// Executa a eliminação da associação.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employeeContract = await _context.EmployeeContracts.FindAsync(id);
            if (employeeContract != null)
            {
                _context.EmployeeContracts.Remove(employeeContract);
                // Adicionamos um segundo SaveChangesAsync aqui para garantir que a remoção é persistida.
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Método auxiliar privado para verificar a existência de uma associação pelo seu ID.
        /// </summary>
        private bool EmployeeContractExists(int id)
        {
            return _context.EmployeeContracts.Any(e => e.EmployeeContractId == id);
        }
    }
}