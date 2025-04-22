using Yape.TransactionService.Domain.Entities;
using Yape.TransactionService.Domain.Interfaces;

namespace Yape.TransactionService.Infrastructure.Repositories;

public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _storage = new();

    public Task SaveAsync(Transaction transaction)
    {
        _storage.Add(transaction);
        return Task.CompletedTask;
    }
}