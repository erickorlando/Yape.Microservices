using Microsoft.Extensions.Logging;
using Yape.TransactionService.Application.Events;
using Yape.TransactionService.Domain.Interfaces;

namespace Yape.TransactionService.Application.EventHandlers;

public class TransactionUpdatedEventHandler(
    ILogger<TransactionUpdatedEventHandler> logger,
    ITransactionRepository transactionRepository)
{
    public async Task Handle(TransactionUpdatedEventMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            $"Handling TransactionUpdated event for TransactionExternalId: {message.TransactionExternalId}");

        // 1. Update the transaction status in the database
        var transaction = await transactionRepository.GetByExternalIdAsync(message.TransactionExternalId, cancellationToken);
        if (transaction != null)
        {
            transaction.Status = message.Status;
            await transactionRepository.UpdateAsync(cancellationToken);
            logger.LogInformation($"Transaction {message.TransactionExternalId} status updated to {message.Status}.");
        }
        else
        {
            logger.LogWarning($"Transaction {message.TransactionExternalId} not found.");
        }
        
    }
}