using Yape.TransactionService.Application.UseCases;
using Yape.TransactionService.Domain.Entities;
using Yape.TransactionService.Domain.Interfaces;

namespace Yape.TransactionService.Application.Services;

public class TransactionService
{
    private readonly ITransactionRepository _repository;
    private readonly ITransactionEventPublisher _eventPublisher;

    public TransactionService(ITransactionRepository repository, ITransactionEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Guid> CreateTransactionAsync(CreateTransactionCommand command)
    {
        var transaction = new Transaction(Guid.NewGuid(), command.Amount);
        await _repository.SaveAsync(transaction);
        await _eventPublisher.PublishAsync(transaction);
        return transaction.Id;
    }
}