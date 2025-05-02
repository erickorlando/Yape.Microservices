using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Yape.AntiFraudService.Application.EventHandlers;
using Yape.AntiFraudService.Application.Events;
using Yape.AntiFraudService.Application.Interfaces;
using Yape.AntiFraudService.Domain.Interfaces;

namespace Yape.AntiFraudService.UnitTests;

public class TransactionCreatedEventHandlerTests
{
    private readonly Mock<IFraudDetectionService> _fraudDetectionServiceMock;
    private readonly Mock<ITransactionStatusProducer> _statusProducerMock;
    private readonly Mock<IAccumulatedValueRepository> _accumulatedValueRepositoryMock;

    private readonly TransactionCreatedEventHandler _handler;
    private readonly string _statusTopic = "transaction.status.update";

    public TransactionCreatedEventHandlerTests()
    {
        var loggerMock = new Mock<ILogger<TransactionCreatedEventHandler>>();
        _fraudDetectionServiceMock = new Mock<IFraudDetectionService>();
        _statusProducerMock = new Mock<ITransactionStatusProducer>();
        _accumulatedValueRepositoryMock = new Mock<IAccumulatedValueRepository>();
        var configurationMock = new Mock<IConfiguration>();

        configurationMock.Setup(c => c["Kafka:StatusUpdateTopic"]).Returns(_statusTopic);

        _handler = new TransactionCreatedEventHandler(
            loggerMock.Object,
            _fraudDetectionServiceMock.Object,
            _statusProducerMock.Object,
            _accumulatedValueRepositoryMock.Object,
            configurationMock.Object);
    }

    [Fact]
    public async Task Handle_TransactionApproved_UpdatesAccumulatedValueAndSendsStatusUpdate()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var sourceAccountId = Guid.NewGuid();
        var value = 100.00m;
        var createdAt = DateTime.UtcNow;

        var message = new TransactionCreatedEventMessage
        {
            TransactionExternalId = transactionId,
            SourceAccountId = sourceAccountId,
            Value = value,
            CreatedAt = createdAt
        };

        _fraudDetectionServiceMock.Setup(s => s.CheckForFraud(
                transactionId, value, sourceAccountId, createdAt))
            .ReturnsAsync(false); // Is not fraud, so should be approved

        var statusUpdateMessage = new StatusMessage(message.TransactionExternalId,"approved");

        // Act
        await _handler.Handle(message, CancellationToken.None);

        // Assert
        _fraudDetectionServiceMock.Verify(s => s.CheckForFraud(
            transactionId, value, sourceAccountId, createdAt), Times.Once);

        _accumulatedValueRepositoryMock.Verify(r => r.UpdateAccumulatedValueForAccountAndDay(
            sourceAccountId, createdAt, value, It.IsAny<CancellationToken>()), Times.Once);

        _statusProducerMock.Verify(p => p.ProduceAsync(
                _statusTopic,
                statusUpdateMessage),
            Times.Once);
    }

    [Fact]
    public async Task Handle_TransactionRejected_DoesNotUpdateAccumulatedValueAndSendsStatusUpdate()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var sourceAccountId = Guid.NewGuid();
        var value = 500.00m;
        var createdAt = DateTime.UtcNow;

        var message = new TransactionCreatedEventMessage
        {
            TransactionExternalId = transactionId,
            SourceAccountId = sourceAccountId,
            Value = value,
            CreatedAt = createdAt
        };

        _fraudDetectionServiceMock.Setup(s => s.CheckForFraud(
                transactionId, value, sourceAccountId, createdAt))
            .ReturnsAsync(true); // It's fraud, so should be rejected
        
        var statusUpdateMessage = new StatusMessage(message.TransactionExternalId,"rejected");

        // Act
        await _handler.Handle(message, CancellationToken.None);

        // Assert
        _fraudDetectionServiceMock.Verify(s => s.CheckForFraud(
            transactionId, value, sourceAccountId, createdAt), Times.Once);

        // Should not update the accumulated value for rejected transactions
        _accumulatedValueRepositoryMock.Verify(r => r.UpdateAccumulatedValueForAccountAndDay(
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()),
            Times.Never);
        
        _statusProducerMock.Verify(p => p.ProduceAsync(
                _statusTopic,
                statusUpdateMessage),
            Times.Once);
    }
}