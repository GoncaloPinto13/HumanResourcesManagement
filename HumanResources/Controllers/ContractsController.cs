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
using HumanResources.ViewModels;


namespace HumanResources.Controllers
{
    /// <summary>
    /// Controlador para gerir Contratos. Lida com o ciclo de vida dos contratos
    /// e a alocação de funcionários a projetos.
    /// </summary>
    public class ContractsController : Controller
    {
        private readonly HumanResourcesContext _context;

        public ContractsController(HumanResourcesContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ação GET para a rota /Contracts.
        /// Apresenta uma lista de todos os contratos.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Carrega os contratos e, para cada um, inclui os dados do Projeto e do Cliente associado.
            // A sintaxe `c.Project.Client` é uma forma de carregar dados relacionados em cascata.
            var humanResourcesContext = _context.Contracts.Include(c => c.Project.Client);
            return View(await humanResourcesContext.ToListAsync());
        }

        /// <summary>
        /// Ação GET para a rota /Contracts/Details/{id}.
        /// Apresenta os detalhes de um contrato específico, incluindo os funcionários alocados.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Consulta complexa para carregar todos os dados necessários:
            var contract = await _context.Contracts
                .Include(c => c.Project.Client)     // Inclui o Cliente através do Projeto.
                .Include(c => c.EmployeeContracts)  // Inclui as associações na tabela de junção.
                    .ThenInclude(ec => ec.Employee) // E para cada associação, inclui os dados do Funcionário.
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null) return NotFound();

            return View(contract);
        }

        /// <summary>
        /// Ação GET para a rota /Contracts/Create.
        /// Apresenta o formulário de criação de contrato.
        /// </summary>
        /// <param name="projectId">Parâmetro opcional que pré-seleciona um projeto no formulário.</param>
        public async Task<IActionResult> Create(int? projectId)
        {
            // Lógica para obter apenas funcionários disponíveis:
            // 1. Carrega todos os funcionários com os seus contratos associados.
            var allEmployees = await _context.Employees
                                             .Include(e => e.EmployeeContracts)
                                             .ThenInclude(ec => ec.Contract)
                                             .ToListAsync();
            // 2. Filtra a lista em memória usando a propriedade calculada 'IsAvailable' do modelo Employee.
            var availableEmployees = allEmployees.Where(e => e.IsAvailable);

            // 3. Prepara o ViewModel para a View.
            var viewModel = new CreateContractViewModel
            {
                AvailableEmployees = new SelectList(availableEmployees, "Id", "Name"),
                Contract = new Contract()
            };

            // Se um ID de projeto foi passado na URL (ex: a partir da página de gestão de um projeto),
            // pré-seleciona esse projeto no formulário para conveniência do utilizador.
            if (projectId.HasValue)
            {
                viewModel.Contract.ProjectId = projectId.Value;
                ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName", projectId.Value);
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName");
            }

            return View(viewModel);
        }

        /// <summary>
        /// Ação POST para a rota /Contracts/Create.
        /// Processa os dados do formulário para criar um Contrato e as suas associações com Funcionários.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContractViewModel viewModel)
        {
            // Removemos manualmente chaves do ModelState que não são preenchidas pelo formulário,
            // mas que poderiam causar falhas de validação desnecessárias.
            ModelState.Remove("AvailableEmployees");
            ModelState.Remove("Contract.Project");
            ModelState.Remove("Contract.EmployeeContracts");

            if (ModelState.IsValid)
            {
                // Passo 1: Adicionar e salvar o Contrato primeiro.
                // Isto é crucial para que o Entity Framework gere um ID para o novo contrato.
                _context.Add(viewModel.Contract);
                await _context.SaveChangesAsync();

                // Passo 2: Se foram selecionados funcionários, iterar sobre os seus IDs.
                if (viewModel.SelectedEmployeeIds != null && viewModel.SelectedEmployeeIds.Any())
                {
                    foreach (var employeeId in viewModel.SelectedEmployeeIds)
                    {
                        // Para cada ID, criar um novo registo na tabela de junção EmployeeContract.
                        var employeeContract = new EmployeeContract
                        {
                            EmployeeId = employeeId,
                            ContractId = viewModel.Contract.Id, // Usar o ID do contrato recém-criado.
                            DurationInDays = viewModel.Contract.DurationInDays // Copiar a duração.
                        };
                        _context.EmployeeContracts.Add(employeeContract);
                    }
                    // Passo 3: Salvar as novas associações na base de dados.
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // Se o modelo for inválido, é necessário repopular as listas de funcionários e projetos
            // antes de devolver a View ao utilizador para correção.
            var allEmployees = await _context.Employees
                                             .Include(e => e.EmployeeContracts)
                                             .ThenInclude(ec => ec.Contract)
                                             .ToListAsync();
            var availableEmployees = allEmployees.Where(e => e.IsAvailable);
            viewModel.AvailableEmployees = new SelectList(availableEmployees, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName", viewModel.Contract.ProjectId);

            return View(viewModel);
        }

        /// <summary>
        /// Ação GET para a rota /Contracts/Edit/{id}.
        /// Apresenta o formulário para editar um contrato.
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null) return NotFound();

            // Popula a dropdown de Projetos para permitir a alteração.
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName", contract.ProjectId);
            return View(contract);
        }

        /// <summary>
        /// Ação POST para a rota /Contracts/Edit/{id}.
        /// Processa os dados do formulário para atualizar um contrato.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServiceDescription,StartDate,ExpirationDate,Value,TermsAndConditions,ProjectId")] Contract contract)
        {
            if (id != contract.Id) return NotFound();

            ModelState.Remove("Project"); // Evitar problemas de validação com a propriedade de navegação.

            if (ModelState.IsValid)
            {
                try
                {
                    // Boa prática: em vez de usar _context.Update(contract), que atualiza todos os campos,
                    // esta abordagem busca a entidade existente e atualiza apenas as propriedades necessárias.
                    // Isto previne a sobreposição acidental de dados que não deveriam ser alterados.
                    var existingContract = await _context.Contracts.FindAsync(id);
                    if (existingContract == null) return NotFound();

                    existingContract.ServiceDescription = contract.ServiceDescription;
                    existingContract.StartDate = contract.StartDate;
                    existingContract.ExpirationDate = contract.ExpirationDate;
                    existingContract.Value = contract.Value;
                    existingContract.TermsAndConditions = contract.TermsAndConditions;
                    existingContract.ProjectId = contract.ProjectId;

                    _context.Update(existingContract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "ProjectName", contract.ProjectId);
            return View(contract);
        }

        /// <summary>
        /// Ação GET para a rota /Contracts/Delete/{id}.
        /// Apresenta a página de confirmação de eliminação.
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .Include(c => c.Project.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null) return NotFound();

            return View(contract);
        }

        /// <summary>
        /// Ação POST para a rota /Contracts/Delete/{id}.
        /// Executa a eliminação do contrato e das suas associações.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.EmployeeContracts) // Carrega as associações para serem removidas.
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null) return NotFound();

            // Passo 1: Remover todos os registos da tabela de junção associados a este contrato.
            // É necessário fazer isto primeiro para não violar as restrições de chave estrangeira.
            if (contract.EmployeeContracts != null)
            {
                _context.EmployeeContracts.RemoveRange(contract.EmployeeContracts);
            }

            // Passo 2: Remover o contrato em si.
            _context.Contracts.Remove(contract);

            // Passo 3: Salvar todas as alterações (remoções) na base de dados.
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Ação GET para a rota /Contracts/ManageContracts/{id}.
        /// Apresenta uma view de gestão para um contrato, permitindo adicionar/remover funcionários.
        /// </summary>
        public async Task<IActionResult> ManageContracts(int? id)
        {
            if (id == null) return NotFound();

            // Carrega o contrato com todos os dados relacionados necessários para a view de gestão.
            var contract = await _context.Contracts
                .Include(c => c.Project.Client)
                .Include(c => c.EmployeeContracts)
                    .ThenInclude(ec => ec.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null) return NotFound();

            // Lógica para popular a dropdown com funcionários disponíveis.
            var employees = await _context.Employees
                .Include(e => e.EmployeeContracts)
                    .ThenInclude(ec => ec.Contract)
                .OrderBy(e => e.Name)
                .ToListAsync();

            var availableEmployees = employees.Where(e => e.IsAvailable);
            ViewBag.EmployeeId = new SelectList(availableEmployees, "Id", "Name");

            return View(contract);
        }

        // --- MÉTODOS PARA GERIR O CICLO DE VIDA DO CONTRATO ---

        /// <summary>
        /// Ação POST para iniciar um contrato.
        /// </summary>
        public async Task<IActionResult> InitServices(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            // Retorna a View InitServices.cshtml
            return View(contract);
        }

        /// <summary>
        /// Ação POST para iniciar um contrato.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitServices(int id, DateTime realStartDate)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            contract.StartDate = realStartDate;
            contract.Status = ContractStatus.InProgress;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageContracts), new { id = id }); // Redireciona de volta para a gestão
        }
        // --- ADICIONADO ---
        // GET: Contracts/PauseServices/5
        // Esta ação é chamada quando clica no link e mostra a página de confirmação.
        public async Task<IActionResult> PauseServices(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            // Retorna a View PauseServices.cshtml
            return View(contract);
        }

        /// <summary>
        /// Ação POST para pausar/retomar um contrato.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PauseServices(int id, bool isOnStandby)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            if (isOnStandby)
            {
                contract.Status = ContractStatus.OnHold;
            }
            else
            {
                contract.Status = ContractStatus.InProgress;
            }

            contract.IsOnStandby = isOnStandby;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageContracts), new { id = id }); // Redireciona de volta para a gestão
        }

        /// <summary>
        /// Ação POST para finalizar um contrato.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinishContract(int id, DateTime realEndDate, decimal realValue)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            // Atualiza os campos do contrato com os valores finais.
            contract.ExpirationDate = realEndDate;
            contract.Value = realValue;
            contract.Status = ContractStatus.Completed;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --- MÉTODOS PARA ADICIONAR/REMOVER FUNCIONÁRIOS DE UM CONTRATO ---

        /// <summary>
        /// Ação GET que normalmente retorna uma Partial View para um modal de adicionar funcionário.
        /// </summary>
        public async Task<IActionResult> AddEmployeeContract(int? id)
        {
            if (id == null) return NotFound();
            // ... (Lógica para preparar os dados para o modal) ...
            var availableEmployees = await _context.Employees
                .Where(e => e.IsAvailable)
                .ToListAsync();

            ViewBag.EmployeeId = new SelectList(availableEmployees, "Id", "Name");
            ViewBag.ContractId = id;

            return PartialView("_AddEmployeeContractModal");
        }

        /// <summary>
        /// Ação POST para adicionar uma nova associação entre um funcionário e um contrato.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployeeContract(int contractId, int employeeId, DateTime startDate, DateTime endDate)
        {
            if (!_context.Contracts.Any(c => c.Id == contractId))
            {
                ModelState.AddModelError("", "Contrato inválido selecionado.");
                return RedirectToAction(nameof(ManageContracts), new { id = contractId });
            }

            var durationInDays = (int)(endDate - startDate).TotalDays;

            var employeeContract = new EmployeeContract
            {
                ContractId = contractId,
                EmployeeId = employeeId,
                StartDate = startDate,
                DurationInDays = durationInDays
            };

            _context.EmployeeContracts.Add(employeeContract);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageContracts), new { id = contractId });
        }

        /// <summary>
        /// Ação POST para remover a associação entre um funcionário e um contrato.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEmployeeContract(int employeeContractId, int contractId)
        {
            var employeeContract = await _context.EmployeeContracts.FindAsync(employeeContractId);
            if (employeeContract != null)
            {
                _context.EmployeeContracts.Remove(employeeContract);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageContracts), new { id = contractId });
        }

        /// <summary>
        /// Método auxiliar privado para verificar a existência de um contrato.
        /// </summary>
        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }
    }
}