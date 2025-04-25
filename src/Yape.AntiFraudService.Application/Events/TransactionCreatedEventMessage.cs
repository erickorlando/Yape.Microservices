namespace Yape.AntiFraudService.Application.Events;

public class TransactionCreatedEventMessage
{
    public Guid TransactionExternalId { get; set; }
    public decimal Value { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid SourceAccountId { get; set; }
    
        
}