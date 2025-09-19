// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
using HumanResources.Areas.Identity.Data; // Importa a classe de utilizador personalizada.
using HumanResources.Data;
using HumanResources.Models;
using HumanResources.ViewModels; // Importa o ViewModel para o formulário de criação.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Importa os gestores do Identity (UserManager, RoleManager).
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.Controllers
{
    /// <summary>
    /// Controlador para gerir as operações CRUD da entidade Funcionário (Employee).
    /// Lida com a criação de utilizadores associados com perfis específicos.
    /// </summary>
    /// 

    [Authorize(Roles = "Admin")] // ADICIONADO: Restringe o acesso a este controller APENAS ao perfil "Admin".
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
        // Prepara o ViewModel com a lista de perfis
        var viewModel = new CreateEmployeeViewModel
        {
            RoleList = await GetRolesSelectListAsync()
        };

        return View(viewModel);
    }

    // POST: Employees/Create
    // Este método é chamado quando você submete o formulário.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new HumanResourcesUser 
            { 
                UserName = model.Email, 
                Email = model.Email, 
                EmailConfirmed = true 
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);

                var employee = new Employee
                {
                    Name = model.Name,
                    Position = model.Position,
                    SpecializationArea= "funcionario",
                    UserId = user.Id
                };
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // --- ESTA É A PARTE CRUCIAL ---
        // Se a validação do modelo falhar, a lista de perfis TEM de ser
        // preenchida novamente antes de devolver a View.
        model.RoleList = await GetRolesSelectListAsync();
        return View(model);
    }

    // ... (Edit, Delete, etc. permanecem iguais) ...

    // --- NOVO MÉTODO PRIVADO PARA EVITAR REPETIÇÃO DE CÓDIGO ---
    private async Task<SelectList> GetRolesSelectListAsync()
    {
        var roles = await _roleManager.Roles
            .Where(r => r.Name == "Employee" || r.Name == "Project Manager")
            .ToListAsync();
        return new SelectList(roles, "Name", "Name");
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