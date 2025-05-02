using Yape.AntiFraudService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Yape.AntiFraudService.Domain.Services;

public class FraudDetectionService : IFraudDetectionService
{
    private readonly IAccumulatedValueRepository _accumulatedValueRepository;
    private readonly ILogger<FraudDetectionService> _logger;

    public FraudDetectionService(
        IAccumulatedValueRepository accumulatedValueRepository,
        ILogger<FraudDetectionService> logger)
    {
        _accumulatedValueRepository = accumulatedValueRepository;
        _logger = logger;
    }

    public async Task<bool> CheckForFraud(Guid transactionExternalId, decimal transactionValue, 
        Guid sourceAccountId, DateTime transactionDate)
    {
        // Rule 1: Transaction value greater than 2000
        if (transactionValue > 2000)
        {
            _logger.LogWarning("Fraud detected for transaction {TransactionId}: Value ({Value}) > 2000.", 
                transactionExternalId, transactionValue);
            return true; // Rejected
        }

        // Rule 2: Accumulated value per day is greater than 20000
        var transactionDateOnly = transactionDate.Date;
        var accumulatedValue = await _accumulatedValueRepository.GetAccumulatedValueForAccountAndDay(sourceAccountId, transactionDateOnly);

        if (accumulatedValue + transactionValue > 20000)
        {
            _logger.LogWarning(
                "Fraud detected for transaction {TransactionId}: Accumulated daily value ({AccumulatedValue} + {TransactionValue}) > 20000 for account {AccountId} on {Date}.", 
                transactionExternalId, accumulatedValue, transactionValue, sourceAccountId, transactionDateOnly.ToShortDateString());
            return true; // Rejected
        }

        _logger.LogInformation("No fraud detected for transaction {TransactionId}.", transactionExternalId);
        return false; // Approved
    }
}