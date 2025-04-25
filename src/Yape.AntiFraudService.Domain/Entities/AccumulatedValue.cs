namespace Yape.AntiFraudService.Domain.Entities;

public class AccumulatedValue
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public DateTime Date { get; set; }      
    public decimal TotalValue { get; set; } 

    public AccumulatedValue()
    {
        Id = Guid.NewGuid(); 
    }
}