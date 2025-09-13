using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResources.Models
{
    [Table("EmployeeContracts")]
    public class EmployeeContract
    {
        [Key]
        public int EmployeeContractId { get; set; }

        // Chaves Estrangeiras para as tabelas Employee e Contract
        public int EmployeeId { get; set; }
        public int ContractId { get; set; }

        // Propriedade para a duração do funcionário no contrato
        [Display(Name = "Duration (days)")]
        [Required(ErrorMessage = "Duration is mandatory.")]
        public int DurationInDays { get; set; }

        // Propriedades de Navegação (referências às classes completas)
        public Employee Employee { get; set; }
        public Contract Contract { get; set; }
    }
}