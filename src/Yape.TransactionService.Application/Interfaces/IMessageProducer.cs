namespace Yape.TransactionService.Application.Interfaces;

public interface IMessageProducer
{
    Task ProduceAsync<T>(string key, T message);
}