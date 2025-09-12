public class Project
{
    public int Id { get; set; }
    public string? ProjectName { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Budget { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public ICollection<Contract> Contracts { get; set; }
    // Adicione esta propriedade para corrigir o erro:
    public ICollection<Employee> Employees { get; set; }
}