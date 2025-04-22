using Yape.TransactionService.Domain.Entities;

namespace Yape.TransactionService.Domain.Interfaces;

public interface ITransactionRepository
{
    Task SaveAsync(Transaction transaction);
}