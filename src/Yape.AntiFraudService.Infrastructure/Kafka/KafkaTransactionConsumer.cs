using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;


namespace Yape.AntiFraudService.Infrastructure.Kafka;

public class KafkaTransactionConsumer : BackgroundService
{
    private readonly ILogger<KafkaTransactionConsumer> _logger;

    public KafkaTransactionConsumer(ILogger<KafkaTransactionConsumer> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            GroupId = "anti-fraud-consumer-group",
            BootstrapServers = "localhost:9092",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("transactions");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var message = JsonSerializer.Deserialize<TransactionMessage>(result.Message.Value)!;

                _logger.LogInformation("Transacción recibida: {Id} por {Amount:C}", message.TransactionId, message.Amount);

                // Anti-fraud logic 
                if (message.Amount > 1000)
                    _logger.LogWarning("Transacción sospechosa detectada: {Id} por {Amount:C}", message.TransactionId, message.Amount);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming message");
            }
        }

        await Task.CompletedTask;
    }
}

public class TransactionMessage
{
    public Guid TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}