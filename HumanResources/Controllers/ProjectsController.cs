// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
using HumanResources.Data;
using HumanResources.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Model.Structures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResources.Controllers
{
    /// <summary>
    /// Controlador para gerir as operações CRUD da entidade Projeto (Project).
    /// </summary>
    /// 

    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly HumanResourcesContext _context;

        /// <summary>
        /// Construtor que injeta o contexto da base de dados (DbContext) para permitir o acesso aos dados.
        /// </summary>
        public ProjectsController(HumanResourcesContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ação GET para a rota /Projects.
        /// Lista todos os projetos, incluindo o nome do cliente associado.
        /// </summary>
        /// 
        [Authorize(Roles = "Admin,Gestor de Projeto,Employee")]
        public async Task<IActionResult> Index()
        {
            // O .Include(p => p.Client) instrui o EF Core a carregar (Eager Loading)
            // os dados do Cliente relacionado com cada Projeto numa única consulta à BD.
            var humanResourcesContext = _context.Projects.Include(p => p.Client);
            return View(await humanResourcesContext.ToListAsync());
        }

        /// <summary>
        /// Ação GET para a rota /Projects/Details/{id}.
        /// Mostra os detalhes de um projeto, incluindo o cliente e a lista de contratos associados.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Client)      // Carrega os dados do Cliente.
                .Include(p => p.Contracts)   // Carrega a coleção de Contratos associados a este projeto.
                .FirstOrDefaultAsync(m => m.Id == id); // Encontra o projeto com o ID correspondente.

            if (project == null) return NotFound();

            return View(project);
        }

        /// <summary>
        /// Ação GET para a rota /Projects/Create.
        /// Apresenta o formulário para criar um novo projeto.
        /// </summary>
        /// 
        [Authorize(Roles = "Admin,Gestor de Projeto")]
        public IActionResult Create()
        {
            // Popula um item no ViewData com uma SelectList de todos os clientes.
            // A View usará isto para renderizar uma dropdown para selecionar o cliente do projeto.
            // Parâmetros: (fonte de dados, valor da opção, texto da opção)
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName");
            return View();
        }

        /// <summary>
        /// Ação POST para a rota /Projects/Create.
        /// Processa os dados do formulário para criar um novo projeto.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        // O atributo [Bind] é uma medida de segurança que especifica explicitamente quais propriedades
        // do modelo 'Project' podem ser preenchidas a partir do formulário. Isto previne ataques de "over-posting".


        [Authorize(Roles = "Admin,Gestor de Projeto")]
        public async Task<IActionResult> Create([Bind("Id,ProjectName,Description,StartDate,DueDate,Budget,ClientId")] Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project); // Adiciona o novo projeto ao contexto do EF.
                await _context.SaveChangesAsync(); // Persiste as alterações na base de dados.
                return RedirectToAction(nameof(Index)); // Redireciona para a página de listagem.
            }
            // Se o modelo for inválido, repopula a dropdown de clientes (pré-selecionando o valor que o utilizador já tinha escolhido)
            // e retorna a view com os erros de validação.
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", project.ClientId);
            return View(project);
        }

        /// <summary>
        /// Ação GET para a rota /Projects/Edit/{id}.
        /// Apresenta o formulário para editar um projeto existente.
        /// </summary>
        /// 
        [Authorize(Roles = "Admin,Gestor de Projeto")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);

            if (project == null) return NotFound();

            // Popula a dropdown de clientes, pré-selecionando o cliente atual do projeto.
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", project.ClientId);
            return View(project);
        }

        /// <summary>
        /// Ação POST para a rota /Projects/Edit/{id}.
        /// Processa os dados do formulário para atualizar um projeto.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]

        [Authorize(Roles = "Admin,Gestor de Projeto")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectName,Description,StartDate,DueDate,Budget,ClientId")] Project project)
        {
            if (id != project.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Trata erros de concorrência (quando outro utilizador altera o mesmo registo).
                    if (!ProjectExists(project.Id))
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
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", project.ClientId);
            return View(project);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ProjectStatus status)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            project.ProjectStatus = status;
            _context.Update(project);
            await _context.SaveChangesAsync();

            // Redireciona de volta para a mesma página de gestão
            return RedirectToAction(nameof(ManageProjects), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        [Authorize(Roles = "Admin,Gestor de Projeto")]
        public async Task<IActionResult> CompleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            project.ProjectStatus = ProjectStatus.Completed;
            _context.Update(project);
            await _context.SaveChangesAsync();

            // Redireciona de volta para a mesma página de gestão
            return RedirectToAction(nameof(ManageProjects), new { id = id });
        }



        /// <summary>
        /// Ação GET para a rota /Projects/Delete/{id}.
        /// Apresenta uma página de confirmação para apagar um projeto.
        /// </summary>
        /// 
        [Authorize(Roles = "Admin,Gestor de Projeto")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Client) // Carrega o cliente para exibir o seu nome na página de confirmação.
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            return View(project);
        }

        /// <summary>
        /// Ação POST para a rota /Projects/Delete/{id}.
        /// Executa a eliminação do projeto.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Passo 1: Carregar o projeto e TODA a sua hierarquia de dependentes
            // (Projetos -> Contratos -> Ligações com Funcionários)
            var project = await _context.Projects
                .Include(p => p.Contracts)
                    .ThenInclude(c => c.EmployeeContracts)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project != null)
            {
                // Passo 2: Iterar sobre cada contrato para apagar as suas associações
                foreach (var contract in project.Contracts)
                {
                    if (contract.EmployeeContracts.Any())
                    {
                        // Remove as ligações entre Contrato e Funcionários
                        _context.EmployeeContracts.RemoveRange(contract.EmployeeContracts);
                    }
                }

                // Passo 3: Apagar os contratos associados ao projeto.
                if (project.Contracts.Any())
                {
                    _context.Contracts.RemoveRange(project.Contracts);
                }

                // Passo 4: Finalmente, apagar o projeto em si.
                _context.Projects.Remove(project);

                // Passo 5: Guardar todas as alterações (remoções) numa única transação.
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Ação GET para a rota /Projects/ManageProjects/{id}.
        /// Apresenta uma view de gestão detalhada para um projeto específico.
        /// </summary>
        /// <param name="id">O ID do projeto a gerir.</param>
        public async Task<IActionResult> ManageProjects(int? id)
        {
            if (id == null) return NotFound();

            // Carrega o projeto com o cliente e os contratos associados.
            var project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Contracts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            // Retorna a view específica "ManageProjects.cshtml", que terá uma UI
            // dedicada para gerir os detalhes deste projeto.
            return View(project);
        }

        /// <summary>
        /// Método auxiliar privado para verificar se um projeto com um determinado ID existe.
        /// </summary>
        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}