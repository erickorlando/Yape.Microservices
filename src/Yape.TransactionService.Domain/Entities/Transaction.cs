namespace Yape.TransactionService.Domain.Entities;

public class Transaction
{
    public Guid TransactionExternalId { get; set; } // Using ExternalId as per requirements 
    public Guid SourceAccountId { get; set; } 
    public Guid TargetAccountId { get; set; } 
    public int TranferTypeId { get; set; }
    public decimal Value { get; set; }
    public string Status { get; set; } = null!; // e.g., "pending", "approved", "rejected" 
    public DateTime CreatedAt { get; set; } 
}