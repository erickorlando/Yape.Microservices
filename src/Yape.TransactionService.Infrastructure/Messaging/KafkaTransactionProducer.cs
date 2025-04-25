using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yape.TransactionService.Application.Interfaces;

namespace Yape.TransactionService.Infrastructure.Messaging;

public class KafkaMessageProducer : IMessageProducer, IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaMessageProducer> _logger;

        // You'll need Kafka connection settings, often from configuration
        public KafkaMessageProducer(IConfiguration configuration, ILogger<KafkaMessageProducer> logger)
        {
            _logger = logger;

            // Configure Kafka producer
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] // Get server address from config
                // Add other configuration settings here (e.g., security, acks)
            };

            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();

            _logger.LogInformation("Kafka producer initialized.");
        }

        public async Task ProduceAsync<T>(string topic, T message)
        {
            try
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                var dr = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = jsonMessage });

                _logger.LogInformation($"Delivered message to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"Delivery failed: {e.Error.Reason}");
                // Depending on your requirements, you might want to re-throw or handle
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while producing message: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            // Flush and dispose the producer
            _producer.Flush(TimeSpan.FromSeconds(10)); // Wait up to 10 seconds for pending messages to be delivered
            _producer.Dispose();
            _logger.LogInformation("Kafka producer disposed.");
        }
    }