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
using HumanResources.ViewModels; // Importa o ViewModel para o formulário de criação.
using Microsoft.AspNetCore.Identity; // Importa os gestores do Identity (UserManager, RoleManager).
using HumanResources.Areas.Identity.Data; // Importa a classe de utilizador personalizada.

namespace HumanResources.Controllers
{
    /// <summary>
    /// Controlador para gerir as operações CRUD da entidade Funcionário (Employee).
    /// Lida com a criação de utilizadores associados com perfis específicos.
    /// </summary>
    public class EmployeesController : Controller
    {
        // --- INJEÇÃO DE DEPENDÊNCIAS ---
        private readonly HumanResourcesContext _context;
        private readonly UserManager<HumanResourcesUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Construtor que injeta as dependências necessárias: o contexto da BD,
        /// o gestor de utilizadores e o gestor de perfis.
        /// </summary>
        public EmployeesController(HumanResourcesContext context, UserManager<HumanResourcesUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Ação GET para a rota /Employees.
        /// Apresenta uma lista de todos os funcionários.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // A consulta carrega os funcionários e os seus dados relacionados de forma encadeada (Eager Loading).
            var employees = await _context.Employees
                .Include(e => e.User)                   // Inclui os dados do utilizador (login/email).
                .Include(e => e.EmployeeContracts)      // Inclui as associações na tabela de junção.
                    .ThenInclude(ec => ec.Contract)     // E para cada associação, inclui os dados do Contrato.
                .ToListAsync();

            return View(employees);
        }

        /// <summary>
        /// Ação GET para a rota /Employees/Details/{id}.
        /// Mostra os detalhes de um funcionário, incluindo os projetos em que está alocado.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Esta consulta complexa carrega a cadeia completa de relações para mostrar os projetos do funcionário.
            var employee = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.EmployeeContracts)
                    .ThenInclude(ec => ec.Contract)
                        .ThenInclude(c => c.Project) // Carrega o Projeto associado a cada Contrato.
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        /// <summary>
        /// Ação GET para a rota /Employees/Create.
        /// Apresenta o formulário de criação de funcionário, com uma lista de perfis para seleção.
        /// </summary>
        public async Task<IActionResult> Create()
        {
            // Obtém os perfis relevantes ("Employee" e "Project Manager") da base de dados.
            var roles = await _roleManager.Roles
                .Where(r => r.Name == "Employee" || r.Name == "Project Manager")
                .ToListAsync();

            // Prepara o ViewModel para a View.
            var viewModel = new CreateEmployeeViewModel
            {
                // Cria um SelectList para ser usado para gerar a dropdown no formulário.
                RoleList = new SelectList(roles, "Name", "Name")
            };

            return View(viewModel);
        }

        /// <summary>
        /// Ação POST para a rota /Employees/Create.
        /// Processa a criação de um novo Funcionário e do seu Utilizador associado com o perfil selecionado.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Passo 1: Criar o objeto User.
                var user = new HumanResourcesUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Passo 2: Se o utilizador foi criado, atribuir-lhe o perfil (role) selecionado no formulário.
                    await _userManager.AddToRoleAsync(user, model.Role);

                    // Passo 3: Criar a entidade Employee e associá-la ao utilizador.
                    var employee = new Employee
                    {
                        Name = model.Name,
                        Position = model.Position,
                        UserId = user.Id // A ligação é feita aqui.
                    };
                    _context.Add(employee);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                // Se a criação do utilizador falhou (ex: email já existe, password não cumpre os requisitos),
                // adiciona os erros ao ModelState para serem exibidos na view.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Se o modelo for inválido, é necessário recarregar a lista de perfis
            // antes de devolver a View ao utilizador para correção.
            var roles = await _roleManager.Roles
                .Where(r => r.Name == "Employee" || r.Name == "Project Manager")
                .ToListAsync();
            model.RoleList = new SelectList(roles, "Name", "Name");

            return View(model);
        }

        /// <summary>
        /// Ação GET para a rota /Employees/Edit/{id}.
        /// Apresenta o formulário para editar os dados de um funcionário.
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        /// <summary>
        /// Ação POST para a rota /Employees/Edit/{id}.
        /// Processa a atualização dos dados do funcionário.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Position,SpecializationArea,UserId")] Employee employee)
        {
            if (id != employee.Id) return NotFound();

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

        /// <summary>
        /// Ação GET para a rota /Employees/Delete/{id}.
        /// Apresenta a página de confirmação para apagar um funcionário.
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .Include(e => e.User) // Carrega o utilizador para mostrar o email na confirmação.
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        /// <summary>
        /// Ação POST para a rota /Employees/Delete/{id}.
        /// Executa a eliminação do funcionário.
        /// NOTA: Esta ação apaga o registo 'Employee', mas não o 'HumanResourcesUser' associado.
        /// </summary>
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

        /// <summary>
        /// Método auxiliar privado para verificar se um funcionário existe.
        /// </summary>
        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}