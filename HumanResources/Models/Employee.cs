// --- INÍCIO DAS IMPORTAÇÕES (USINGS) ---
// Importa a classe de utilizador personalizada do ASP.NET Core Identity.
using HumanResources.Areas.Identity.Data;
// Importa tipos básicos do .NET.
using System;
using System.Collections.Generic;
// Importa atributos para validação de dados e mapeamento do esquema da base de dados.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

// Importa funcionalidades do LINQ para realizar consultas em coleções.
using System.Linq;

// O namespace agrupa as classes de modelo, mantendo o código organizado.
namespace HumanResources.Models
{
    /// <summary>
    /// Representa a entidade Funcionário (Employee) no sistema.
    /// O atributo [Table("Employees")] mapeia esta classe para a tabela "Employees" na base de dados.
    /// </summary>
    [Table("Employees")]
    public class Employee
    {
        // NOTA: Embora o Entity Framework Core reconheça "Id" ou "EmployeeId" como chave primária por convenção,
        // é uma boa prática adicionar o atributo [Key] para tornar a intenção explícita.
        // [Key]
        [Column("EmployeeId")] // Renomeia a coluna na base de dados para "EmployeeId".
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do funcionário é obrigatório.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
        [Column("FullName")] // Renomeia a coluna para "FullName".
        // O atributo [Display] personaliza o nome do campo que aparece na interface do utilizador (ex: em labels de formulários).
        [Display(Name = "Nome Completo")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O cargo é obrigatório.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "O cargo deve ter entre 2 e 50 caracteres.")]
        [Column("Position")]
        [Display(Name = "Cargo")]
        public string Position { get; set; }

        [StringLength(100, ErrorMessage = "A área de especialização não pode exceder 100 caracteres.")]
        [Column("SpecializationArea")]
        [Display(Name = "Área de Especialização")]
        [AllowNull]
        public string SpecializationArea { get; set; } 

        // --- Propriedades Calculadas ---
        // Estas propriedades não existem como colunas na base de dados (devido ao [NotMapped]).
        // Os seus valores são calculados em tempo de execução com base noutras propriedades do modelo.

        [NotMapped] // Impede que o EF Core tente criar uma coluna para esta propriedade.
        [Display(Name = "Total de Contratos")]
        // Usa uma expressão "body" (=>) para um getter conciso.
        // `EmployeeContracts?` (operador null-conditional) acede a .Count apenas se a coleção não for nula.
        // `?? 0` (operador de coalescência nula) retorna 0 se o resultado da esquerda for nulo.
        public int TotalContracts => EmployeeContracts?.Count ?? 0;

        [NotMapped]
        [Display(Name = "Contratos Ativos")]
        // Conta apenas os registos em EmployeeContracts cujo contrato associado ainda não expirou.
        public int ActiveContracts => EmployeeContracts?.Count(ec => ec.Contract.ExpirationDate >= DateTime.Now) ?? 0;

        [NotMapped]
        [Display(Name = "Disponível")]
        // Um funcionário está disponível se o seu número de contratos ativos for zero.
        public bool IsAvailable => ActiveContracts == 0;

        // --- Relações ---

        // CHAVE ESTRANGEIRA que liga à tabela AspNetUsers.
        public string UserId { get; set; }

        // PROPRIEDADE DE NAVEGAÇÃO para aceder ao utilizador associado.
        // `virtual` permite o "lazy loading".
        [ForeignKey("UserId")]
        public virtual HumanResourcesUser User { get; set; }

        // PROPRIEDADE DE NAVEGAÇÃO DE COLEÇÃO (Lado "Um" da relação 1-N).
        // Um funcionário tem uma coleção de "vínculos contratuais" (EmployeeContracts).
        // É através desta coleção que se estabelece a relação Muitos-para-Muitos com os Contratos.
        public ICollection<EmployeeContract> EmployeeContracts { get; set; } = new List<EmployeeContract>();

        // Esta propriedade define uma relação Muitos-para-Muitos (N-N) DIRETA com Projetos.
        // Para que isto funcione, o EF Core criaria automaticamente uma tabela de junção
        // na base de dados (ex: "EmployeeProject") para ligar Employees e Projects.
        // NOTA: Isto representa uma forma de relação diferente da que passa por Contratos.
        public ICollection<Project> Projects { get; set; } = new List<Project>();

        // --- Métodos Auxiliares ---
        // Métodos que encapsulam lógica de negócio e facilitam a verificação de estados.

        /// <summary>
        /// Verifica se o funcionário está envolvido num contrato específico.
        /// </summary>
        /// <param name="contractId">O ID do contrato a verificar.</param>
        /// <returns>True se o funcionário estiver associado ao contrato, caso contrário false.</returns>
        public bool IsInvolvedInContract(int contractId)
        {
            // Usa LINQ (.Any) para verificar eficientemente se existe algum `EmployeeContract`
            // que satisfaça a condição, sem precisar de iterar por toda a coleção.
            return EmployeeContracts?.Any(ec => ec.ContractId == contractId) ?? false;
        }

        /// <summary>
        /// Verifica se o funcionário tem algum contrato ativo no momento.
        /// </summary>
        /// <returns>True se houver pelo menos um contrato ativo, caso contrário false.</returns>
        public bool HasActiveContracts()
        {
            return EmployeeContracts?.Any(ec => ec.Contract.ExpirationDate >= DateTime.Now) ?? false;
        }
    }
}