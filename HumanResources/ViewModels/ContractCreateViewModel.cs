using HumanResources.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HumanResources.ViewModels
{
    public class CreateContractViewModel
    {
        public Contract Contract { get; set; }

        [Required(ErrorMessage = "You must select at least one employee.")]
        [Display(Name = "Available Employees")]
        public List<int> SelectedEmployeeIds { get; set; } = new List<int>();

        [BindNever] // <-- ADICIONE ESTE ATRIBUTO
        public SelectList AvailableEmployees { get; set; }
    }
}