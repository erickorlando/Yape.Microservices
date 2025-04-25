using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yape.AntiFraudService.Application.Interfaces;

namespace Yape.AntiFraudService.Infrastructure.Messaging;

 public class KafkaTransactionStatusProducer : ITransactionStatusProducer, IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaTransactionStatusProducer> _logger;

        public KafkaTransactionStatusProducer(IConfiguration configuration, ILogger<KafkaTransactionStatusProducer> logger)
        {
            _logger = logger;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] // Get server address from config
            };

            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();

            _logger.LogInformation("Kafka transaction status producer initialized.");
        }

        public async Task ProduceAsync<T>(string topic, T message)
        {
            try
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                var dr = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = jsonMessage });

                _logger.LogInformation($"Delivered transaction status message to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"Transaction status delivery failed: {e.Error.Reason}");
                // Depending on your requirements, you might want to re-throw or handle
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while producing transaction status message: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            // Flush and dispose the producer
            _producer.Flush(TimeSpan.FromSeconds(10)); // Wait up to 10 seconds for pending messages to be delivered
            _producer.Dispose();
            _logger.LogInformation("Kafka transaction status producer disposed.");
        }
    }