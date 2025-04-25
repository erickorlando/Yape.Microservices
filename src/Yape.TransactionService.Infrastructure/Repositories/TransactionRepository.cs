using Microsoft.EntityFrameworkCore;
using Yape.TransactionService.Domain.Entities;
using Yape.TransactionService.Domain.Interfaces;
using Yape.TransactionService.Infrastructure.Persistence;

namespace Yape.TransactionService.Infrastructure.Repositories;

public class TransactionRepository(ApplicationDbContext context) : ITransactionRepository
{
    public async Task SaveAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        await context.Set<Transaction>().AddAsync(transaction, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Transaction?> GetByExternalIdAsync(Guid messageTransactionExternalId, CancellationToken cancellationToken)
    {
        return await context.Set<Transaction>()
            .FirstOrDefaultAsync(t => t.TransactionExternalId == messageTransactionExternalId, cancellationToken);
    }

    public async Task UpdateAsync(CancellationToken cancellationToken = default)
    {
         await context.SaveChangesAsync(cancellationToken);
    }
}