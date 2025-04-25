using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yape.AntiFraudService.Application.Events;
using Yape.AntiFraudService.Application.Interfaces;
using Yape.AntiFraudService.Domain.Interfaces;
using Yape.AntiFraudService.Domain.Services;

namespace Yape.AntiFraudService.Application.EventHandlers;

public class TransactionCreatedEventHandler
{
    private readonly ILogger<TransactionCreatedEventHandler> _logger;
    private readonly FraudDetectionService _fraudDetectionService; 
    private readonly ITransactionStatusProducer _statusProducer;   

    private readonly IAccumulatedValueRepository _accumulatedValueRepository;
    private readonly IConfiguration _configuration;

    public TransactionCreatedEventHandler(
        ILogger<TransactionCreatedEventHandler> logger,
        FraudDetectionService fraudDetectionService,
        ITransactionStatusProducer statusProducer,
        IAccumulatedValueRepository accumulatedValueRepository,
        IConfiguration configuration)
    {
        _logger = logger;
        _fraudDetectionService = fraudDetectionService;
        _statusProducer = statusProducer;
        _accumulatedValueRepository = accumulatedValueRepository;
        _configuration = configuration;
    }

    public async Task Handle(TransactionCreatedEventMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            $"Handling TransactionCreated event for TransactionExternalId: {message.TransactionExternalId}");

        // 1. Perform anti-fraud validation
        var isRejected = await _fraudDetectionService.CheckForFraud(message.TransactionExternalId, message.Value,
            message.SourceAccountId, message.CreatedAt);

        // 2. Determine new status and send update message
        string newStatus = isRejected ? "rejected" : "approved";
        
        // 3. If the transaction is APPROVED, update the accumulated value
        if (newStatus == "approved")
        {
            // Call the repository method to update the accumulated value for the account and day
            await _accumulatedValueRepository.UpdateAccumulatedValueForAccountAndDay(
                message.SourceAccountId,
                message.CreatedAt,
                message.Value, cancellationToken);

            _logger.LogInformation($"Accumulated value updated for account {message.SourceAccountId} on {message.CreatedAt.Date.ToShortDateString()} with value {message.Value}.");
        }

        _logger.LogInformation($"Transaction {message.TransactionExternalId} is {newStatus}.");

        // Send a message back to the Transaction service to update the status
        var statusUpdateMessage = new
        {
            message.TransactionExternalId,
            Status = newStatus
        };
        await _statusProducer.ProduceAsync(_configuration["Kafka:StatusUpdateTopic"]!, statusUpdateMessage); 

        _logger.LogInformation(
            $"Finished handling TransactionCreated event for TransactionExternalId: {message.TransactionExternalId}");
    }
}