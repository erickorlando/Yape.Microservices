using System.Text.Json;
using Confluent.Kafka;
using Yape.TransactionService.Domain.Entities;
using Yape.TransactionService.Domain.Interfaces;

namespace Yape.TransactionService.Infrastructure.Kafka;

public class KafkaTransactionProducer : ITransactionEventPublisher
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic = "transactions";

    public KafkaTransactionProducer()
    {
        var config = new ProducerConfig
        {
#if DEBUG   
            BootstrapServers = "localhost:9092"
#else
            BootstrapServers = "kafka:29092"
#endif
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }


    public async Task PublishAsync(Transaction transaction)
    {
        var message = JsonSerializer.Serialize(new
        {
            TransactionId = transaction.Id,
            Amount = transaction.Amount,
            CreatedAt = transaction.CreatedAt
        });

        await _producer.ProduceAsync(_topic, new Message<Null, string>
        {
            Value = message
        });
    }
}