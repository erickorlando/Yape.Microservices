using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yape.TransactionService.Application.EventHandlers;
using Yape.TransactionService.Application.Events;

namespace Yape.TransactionService.Infrastructure.Messaging;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory; 
    private readonly IConfiguration _configuration;

    public KafkaConsumerService(
        IConfiguration configuration,
        ILogger<KafkaConsumerService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = configuration["Kafka:GroupId"],  
            AutoOffsetReset = AutoOffsetReset.Earliest 
        };

        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

        _logger.LogInformation("Kafka consumer initialized.");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => ConsumeMessages(stoppingToken), stoppingToken);
    }

    private void ConsumeMessages(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_configuration["Kafka:StatusUpdateTopic"]!); // Subscribe to the topic

        _logger.LogInformation($"Kafka consumer subscribed to topic '{_configuration["Kafka:StatusUpdateTopic"]}'.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);

                _logger.LogInformation($"Received message at: {consumeResult.TopicPartitionOffset}");

                // Deserialize the message
                var message = JsonSerializer.Deserialize<TransactionUpdatedEventMessage>(consumeResult.Message.Value);

                if (message != null)
                {
                    // Process the message using an event handler
                    ProcessMessage(message, stoppingToken).GetAwaiter()
                        .GetResult(); // Process asynchronously, block until finished
                }
            }
            catch (OperationCanceledException)
            {
                // Consumer was stopped
                break;
            }
            catch (ConsumeException e)
            {
                _logger.LogError($"Consume error: {e.Error.Reason}");
                // Handle specific errors or retry logic here
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unexpected error occurred: {ex.Message}");
                // Handle other exceptions
            }
        }

        _consumer.Close(); // Commit offsets and leave the group cleanly
        _logger.LogInformation("Kafka consumer shut down.");
    }

    private async Task ProcessMessage(TransactionUpdatedEventMessage message, CancellationToken stoppingToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            // Resolve the event handler from the service provider
            var eventHandler = scope.ServiceProvider.GetRequiredService<TransactionUpdatedEventHandler>();

            // Handle the event
            await eventHandler.Handle(message, stoppingToken);
        }
    }

    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}