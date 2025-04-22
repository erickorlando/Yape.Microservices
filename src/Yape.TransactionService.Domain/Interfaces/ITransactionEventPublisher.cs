using Yape.TransactionService.Domain.Entities;

namespace Yape.TransactionService.Domain.Interfaces;

public interface ITransactionEventPublisher
{
    Task PublishAsync(Transaction transaction);
}