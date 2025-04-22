namespace Yape.TransactionService.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Transaction(Guid id, decimal amount)
    {
        Id = id;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
    }
}