using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResources.Models
{
    [Table("Contracts")]
    public class Contract
    {
        [Key]
        [Column("ContractId")]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Column("ServiceDescription")]
        public string ServiceDescription { get; set; }

        [Column("StartDate", TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Column("ExpirationDate", TypeName = "date")]
        public DateTime ExpirationDate { get; set; }

        [Column("Value", TypeName = "decimal(18, 2)")]
        public decimal Value { get; set; }

        [Column("TermsAndConditions", TypeName = "nvarchar(MAX)")]
        public string TermsAndConditions { get; set; }

        // --- Relações ---

        // Relação N-1: Um Contrato pertence a UM Cliente
        [Required]
        public int ClientId { get; set; }
        public Client Client { get; set; }

        // Relação 1-1: Um Contrato pertence a UM Projeto
        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        // Relação N-N (Muitos-para-Muitos) direta com Funcionários
        public ICollection<EmployeeContract> EmployeeContracts { get; set; } = new List<EmployeeContract>();

        // Propriedade calculada para a duração do contrato em dias
        [NotMapped]
        public int DurationInDays => (int)(ExpirationDate - StartDate).TotalDays;

        // Método auxiliar para verificar se o contrato está ativo
        public bool IsActive()
        {
            return ExpirationDate >= DateTime.Now;
        }
    }
}