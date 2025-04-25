using Yape.TransactionService.Domain.Entities;

namespace Yape.TransactionService.Domain.Interfaces;

public interface ITransactionRepository
{
    Task SaveAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByExternalIdAsync(Guid messageTransactionExternalId, CancellationToken cancellationToken);
}