namespace Yape.TransactionService.API.Models;

public class CreateTransactionRequest
{
    public Guid SourceAccountId { get; set; }
    public Guid TargetAccountId { get; set; }
    public int TranferTypeId { get; set; }
    public decimal Value { get; set; }
}