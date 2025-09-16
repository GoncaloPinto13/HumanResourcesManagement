//using Microsoft.AspNetCore.Mvc.Rendering;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;

//namespace HumanResources.ViewModels
//{
//    public class ContractCreateViewModel
//    {
//        // Propriedades do Contrato que vêm do formulário
//        [Required]
//        [StringLength(200)]
//        public string ServiceDescription { get; set; }

//        public DateTime StartDate { get; set; }

//        public DateTime ExpirationDate { get; set; }

//        [DataType(DataType.Currency)]
//        public decimal Value { get; set; }

//        public bool TermsAndConditions { get; set; }

//        [Required]
//        public int ProjectId { get; set; }

//        // --- Propriedades para a seleção de Funcionários ---

//        // Para receber os IDs dos funcionários selecionados no formulário
//        [Required(ErrorMessage = "Deve selecionar pelo menos um funcionário.")]
//        public List<int> SelectedEmployeeIds { get; set; } = new List<int>();

//        // Para preencher a dropdown list na View com os funcionários disponíveis
//        public SelectList? AvailableProjects { get; set; }
//        public MultiSelectList? AvailableEmployees { get; set; }
//    }
//}