using System.ComponentModel.DataAnnotations;

namespace HumanResources.ViewModels
{
    public class EditClientViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da empresa é obrigatório.")]
        [StringLength(100)]
        [Display(Name = "Nome da Empresa")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "O NIF é obrigatório.")]
        [StringLength(9)]
        public string Nif { get; set; }
    }
}