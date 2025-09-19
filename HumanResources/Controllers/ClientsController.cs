// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
using HumanResources.Areas.Identity.Data; // Importa a nossa classe de utilizador personalizada.
using HumanResources.Data;
using HumanResources.Models;
using HumanResources.ViewModels; // Importa o ViewModel usado para o formulário de criação.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Importa as classes de gestão do Identity, como o UserManager.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HumanResources.Controllers
{
    /// <summary>
    /// Controlador para gerir as operações CRUD da entidade Cliente.
    /// Integra-se com o Identity para criar e gerir os utilizadores associados aos clientes.
    /// </summary>
    /// 

    [Authorize(Roles = "Admin")] // ADICIONADO: Restringe o acesso a este controller APENAS ao perfil "Admin".
    public class ClientsController : Controller
    {
        // --- INJEÇÃO DE DEPENDÊNCIAS ---
        private readonly HumanResourcesContext _context;
        private readonly UserManager<HumanResourcesUser> _userManager;

        /// <summary>
        /// Construtor que recebe as dependências necessárias através da Injeção de Dependências do ASP.NET Core.
        /// </summary>
        /// <param name="context">O contexto da base de dados para aceder às tabelas.</param>
        /// <param name="userManager">O serviço do Identity para gerir operações de utilizadores (criar, apagar, etc.).</param>
        public ClientsController(HumanResourcesContext context, UserManager<HumanResourcesUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Ação GET para a rota /Clients ou /Clients/Index.
        /// Apresenta uma lista de todos os clientes registados.
        /// </summary>
        /// <returns>Uma View com a lista de clientes.</returns>
        public async Task<IActionResult> Index()
        {
            // O método .Include(c => c.User) instrui o Entity Framework a carregar também
            // os dados do utilizador associado a cada cliente (Eager Loading).
            // Sem isto, a propriedade 'client.User' seria nula.
            var clients = await _context.Clients.Include(c => c.User).ToListAsync();
            return View(clients);
        }

        /// <summary>
        /// Ação GET para a rota /Clients/Details/{id}.
        /// Apresenta os detalhes de um cliente específico.
        /// </summary>
        /// <param name="id">O ID do cliente a ser visualizado.</param>
        /// <returns>Uma View com os detalhes do cliente ou NotFound se não for encontrado.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients
                .Include(c => c.User) // Carrega também os dados do utilizador associado.
                .FirstOrDefaultAsync(m => m.Id == id); // Procura o cliente pelo ID.

            if (client == null) return NotFound();

            return View(client);
        }

        /// <summary>
        /// Ação GET para a rota /Clients/Create.
        /// Apresenta o formulário para a criação de um novo cliente.
        /// </summary>
        /// <returns>A View do formulário de criação.</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Ação POST para a rota /Clients/Create.
        /// Processa os dados submetidos pelo formulário para criar um novo Cliente e um Utilizador associado.
        /// </summary>
        /// <param name="model">ViewModel que contém todos os dados do formulário.</param>
        [HttpPost] // Especifica que esta ação responde a pedidos HTTP POST.
        [ValidateAntiForgeryToken] // Protege contra ataques CSRF (Cross-Site Request Forgery).
        public async Task<IActionResult> Create(CreateClientViewModel model)
        {
            // Verifica se os dados recebidos cumprem as regras de validação definidas no ViewModel.
            if (ModelState.IsValid)
            {
                // Passo 1: Criar o objeto de utilizador (IdentityUser) primeiro.
                var user = new HumanResourcesUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                // Passo 2: Verificar se a criação do utilizador foi bem-sucedida.
                if (result.Succeeded)
                {
                    // Passo 2a: Atribuir o perfil "Cliente" ao novo utilizador.
                    await _userManager.AddToRoleAsync(user, "Cliente");

                    // Passo 3: Criar a entidade Cliente e ligá-la ao utilizador recém-criado.
                    var client = new Client
                    {
                        CompanyName = model.CompanyName,
                        Nif = model.Nif,
                        UserId = user.Id // A ligação é feita aqui, preenchendo a chave estrangeira.
                    };

                    _context.Add(client); // Adiciona o novo cliente ao contexto do EF.
                    await _context.SaveChangesAsync(); // Guarda as alterações na base de dados.
                    return RedirectToAction(nameof(Index)); // Redireciona para a lista de clientes.
                }

                // Se a criação do utilizador falhou, adiciona os erros ao ModelState para serem exibidos na view.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // Se o modelo não for válido, retorna à view com os dados preenchidos para que o utilizador os possa corrigir.
            return View(model);
        }

        /// <summary>
        /// Ação GET para a rota /Clients/Edit/{id}.
        /// Apresenta o formulário para editar um cliente existente.
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            // Mapear a entidade Client para o EditClientViewModel
            var viewModel = new EditClientViewModel
            {
                Id = client.Id,
                CompanyName = client.CompanyName,
                Nif = client.Nif
            };

            return View(viewModel);
        }

        // POST: Clients/Edit/5
        // Este método agora recebe o ViewModel, valida-o, e depois atualiza a entidade na BD.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditClientViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            // A validação agora funciona corretamente porque o ViewModel só tem os campos do formulário.
            if (ModelState.IsValid)
            {
                try
                {
                    // Vai buscar o cliente original e completo à base de dados.
                    var clientToUpdate = await _context.Clients.FindAsync(id);
                    if (clientToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Atualiza apenas as propriedades que vieram do formulário.
                    // O UserId e outros campos são preservados.
                    clientToUpdate.CompanyName = viewModel.CompanyName;
                    clientToUpdate.Nif = viewModel.Nif;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(viewModel.Id))
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
            // Se o modelo for inválido, retorna à view com o viewModel para correção.
            return View(viewModel);
        }

        /// <summary>
        /// Ação GET para a rota /Clients/Delete/{id}.
        /// Apresenta uma página de confirmação para apagar um cliente.
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients
                .Include(c => c.User) // Carrega os dados do utilizador para exibição na página de confirmação.
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null) return NotFound();

            return View(client);
        }

        /// <summary>
        /// Ação POST para a rota /Clients/Delete/{id}.
        /// Executa a eliminação do cliente.
        /// </summary>
        [HttpPost, ActionName("Delete")] // Mapeia esta ação para o nome "Delete", permitindo que o formulário a chame.
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client); // Marca a entidade para ser removida.
                await _context.SaveChangesAsync(); // Aplica a remoção na base de dados.
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Método auxiliar privado para verificar se um cliente com um determinado ID existe.
        /// </summary>
        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id); // .Any() é uma forma eficiente de verificar a existência.
        }
    }
}