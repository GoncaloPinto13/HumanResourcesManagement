// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
// Importa o modelo de dados 'Contract'.
using HumanResources.Models;
// Importa atributos relacionados com o 'model binding' do ASP.NET Core.
using Microsoft.AspNetCore.Mvc.ModelBinding;
// Importa a classe 'SelectList', usada para popular dropdowns e listas em HTML.
using Microsoft.AspNetCore.Mvc.Rendering;
// Importa tipos básicos do .NET.
using System.Collections.Generic;
// Importa atributos de validação de dados.
using System.ComponentModel.DataAnnotations;

// O namespace agrupa os ViewModels, separando-os dos modelos de dados do domínio.
namespace HumanResources.ViewModels
{
    /// <summary>
    /// ViewModel para a view de criação de um novo Contrato.
    /// Encapsula todos os dados necessários para que o formulário de criação funcione:
    /// o objeto do contrato em si, a lista de IDs de funcionários selecionados pelo utilizador,
    /// e a lista de funcionários disponíveis para serem selecionados.
    /// </summary>
    public class CreateContractViewModel
    {
        /// <summary>
        /// Propriedade que irá conter o objeto do Contrato a ser criado.
        /// Os campos do formulário relacionados com os dados do contrato (ex: ServiceDescription, StartDate)
        /// serão ligados (bound) a esta propriedade.
        /// </summary>
        public Contract Contract { get; set; }

        /// <summary>
        /// Propriedade que irá receber a lista de IDs dos funcionários selecionados
        /// no formulário (provavelmente de uma caixa de seleção múltipla).
        /// </summary>
        [Required(ErrorMessage = "Deve selecionar pelo menos um funcionário.")] // Garante que o utilizador seleciona pelo menos um funcionário.
        [Display(Name = "Funcionários Disponíveis")] // Define o texto do 'label' para este campo na UI.
        public List<int> SelectedEmployeeIds { get; set; } = new List<int>();

        /// <summary>
        /// Propriedade que irá fornecer a lista de funcionários disponíveis PARA a View,
        /// para que ela possa renderizar o controlo de seleção (ex: <select>).
        /// O atributo [BindNever] é uma medida de segurança CRUCIAL. Ele diz ao ASP.NET Core
        /// para NUNCA preencher esta propriedade com dados vindos de um pedido HTTP (POST/GET).
        /// Isto previne ataques de "over-posting", onde um utilizador malicioso poderia tentar
        /// manipular a lista de opções submetida. Esta propriedade é apenas para "sair" do
        // servidor para a view, não para "entrar" dados do utilizador para o servidor.
        /// </summary>
        [BindNever]
        public SelectList AvailableEmployees { get; set; }
    }
}