namespace Yape.AntiFraudService.Application.Interfaces;

public interface ITransactionStatusProducer
{
    Task ProduceAsync<T>(string topic, T message);
}