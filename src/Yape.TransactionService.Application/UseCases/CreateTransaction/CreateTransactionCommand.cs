using MediatR;

namespace Yape.TransactionService.Application.UseCases.CreateTransaction;

public class CreateTransactionCommand : IRequest<Guid>
{
    public Guid SourceAccountId { get; set; }
    public Guid TargetAccountId { get; set; }
    public int TranferTypeId { get; set; }
    public decimal Value { get; set; }
}