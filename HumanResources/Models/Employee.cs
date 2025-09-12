using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace HumanResources.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Column("EmployeeId")]
        public int Id { get; set; }

        [Required(ErrorMessage = "The employee's name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "The name must have between 2 and 100 characters.")]
        [Column("FullName")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The position is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "The position must have between 2 and 50 characters.")]
        [Column("Position")]
        [Display(Name = "Position")]
        public string Position { get; set; }

        [StringLength(100, ErrorMessage = "The specialization area cannot exceed 100 characters.")]
        [Column("SpecializationArea")]
        [Display(Name = "Specialization Area")]
        public string SpecializationArea { get; set; }

        // Calculated properties now based on the EmployeeContract join table
        [NotMapped]
        [Display(Name = "Total Contracts")]
        public int TotalContracts => EmployeeContracts?.Count ?? 0;

        [NotMapped]
        [Display(Name = "Active Contracts")]
        public int ActiveContracts => EmployeeContracts?.Count(ec => ec.Contract.ExpirationDate >= DateTime.Now) ?? 0;

        [NotMapped]
        [Display(Name = "Available")]
        public bool IsAvailable => ActiveContracts == 0;

        // --- Relação de Navegação ---
        // Relação 1-N (Um funcionário tem muitos EmployeeContracts)
        public ICollection<EmployeeContract> EmployeeContracts { get; set; } = new List<EmployeeContract>();

        // --- Métodos Auxiliares ---
        // A lógica agora é baseada na nova classe de junção
        public bool IsInvolvedInContract(int contractId)
        {
            return EmployeeContracts?.Any(ec => ec.ContractId == contractId) ?? false;
        }

        public bool HasActiveContracts()
        {
            return EmployeeContracts?.Any(ec => ec.Contract.ExpirationDate >= DateTime.Now) ?? false;
        }
    }
}