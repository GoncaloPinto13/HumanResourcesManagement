using System.ComponentModel.DataAnnotations;          // DataAnnotations: validação de propriedades (Required, StringLength, etc.)
using System.ComponentModel.DataAnnotations.Schema;   // Mapeamento explícito para tabela/colunas (Table, Column)

namespace HumanResources.Models
{
    [Table("Contracts")] // Mapeia a entidade para a tabela "Contracts"
    public class Contract
    {
        [Key]                       // Chave primária
        [Column("ContractId")]      // Nome da coluna na BD
        public int Id { get; set; }

        [Required]                  // Obrigatório
        [StringLength(200)]         // Máximo 200 caracteres
        [Column("ServiceDescription")]
        public string ServiceDescription { get; set; }

        [Column("StartDate", TypeName = "date")] // Armazenado como 'date' (sem hora) no SQL; em C# continua DateTime (hora 00:00:00)
        public DateTime StartDate { get; set; }

        [Column("ExpirationDate", TypeName = "date")] // Igual ao acima; útil para datas sem tempo
        public DateTime ExpirationDate { get; set; }

        [Column("Value", TypeName = "decimal(18, 2)")] // Precisão/escala para valores monetários; evita float/double
        public decimal Value { get; set; }

        [Column("TermsAndConditions", TypeName = "nvarchar(MAX)")] // Texto grande (até MAX); sem validação de tamanho aqui
        public string TermsAndConditions { get; set; }

        // --- Relações ---

        // Relação N-1: Um Contrato pertence a UM Cliente (muitos contratos podem referir o mesmo cliente)
        [Required]                 // FK obrigatória
        public int ClientId { get; set; }
        public Client Client { get; set; } // Navegação para o Cliente

        
        [Required]                 // FK obrigatória
        public int ProjectId { get; set; }
        public Project Project { get; set; } // Navegação para o Projeto

    }
}
