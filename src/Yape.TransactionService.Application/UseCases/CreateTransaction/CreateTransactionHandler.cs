using MediatR;
using Microsoft.Extensions.Configuration;
using Yape.TransactionService.Application.Interfaces;
using Yape.TransactionService.Domain.Entities;
using Yape.TransactionService.Domain.Interfaces;

namespace Yape.TransactionService.Application.UseCases.CreateTransaction;

public class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, Guid>
{
    private readonly IMessageProducer _messageProducer;
    private readonly ITransactionRepository _repository;
    private readonly IConfiguration _configuration;

    public CreateTransactionHandler(IMessageProducer messageProducer, ITransactionRepository repository, 
        IConfiguration configuration)
    {
        _messageProducer = messageProducer;
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = new Transaction
        {
            TransactionExternalId = Guid.NewGuid(), // Generate a unique ID
            SourceAccountId = request.SourceAccountId,
            TargetAccountId = request.TargetAccountId,
            TranferTypeId = request.TranferTypeId,
            Value = request.Value,
            Status = "pending", // Set initial status to pending 
            CreatedAt = DateTime.UtcNow
        };

        await _repository.SaveAsync(transaction, cancellationToken);

        var eventData = new
        {
            transaction.TransactionExternalId, transaction.Value, transaction.CreatedAt,
            transaction.SourceAccountId
        };
        await _messageProducer.ProduceAsync(_configuration["Kafka:CreatedTransactionTopic"]!, eventData); 

        return transaction.TransactionExternalId; 
    }
}