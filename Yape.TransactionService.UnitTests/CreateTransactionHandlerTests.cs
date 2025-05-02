using Microsoft.Extensions.Configuration;
using Moq;
using Yape.TransactionService.Application.Interfaces;
using Yape.TransactionService.Application.UseCases.CreateTransaction;
using Yape.TransactionService.Domain.Entities;
using Yape.TransactionService.Domain.Interfaces;

namespace Yape.TransactionService.UnitTests;

public class CreateTransactionHandlerTests
{
    private readonly Mock<IMessageProducer> _mockMessageProducer;
    private readonly Mock<ITransactionRepository> _mockRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly CreateTransactionHandler _handler;
    private readonly string _kafkaTopic = "transactions.created";

    public CreateTransactionHandlerTests()
    {
        _mockMessageProducer = new Mock<IMessageProducer>();
        _mockRepository = new Mock<ITransactionRepository>();
        _mockConfiguration = new Mock<IConfiguration>();

        _mockConfiguration.Setup(c => c["Kafka:CreatedTransactionTopic"]).Returns(_kafkaTopic);

        _handler = new CreateTransactionHandler(
            _mockMessageProducer.Object,
            _mockRepository.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateTransactionWithCorrectData()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TranferTypeId = 2,
            Value = 150.75m
        };

        Transaction capturedTransaction = null;
        _mockRepository
            .Setup(r => r.SaveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Callback<Transaction, CancellationToken>((t, ct) => capturedTransaction = t)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal(command.SourceAccountId, capturedTransaction.SourceAccountId);
        Assert.Equal(command.TargetAccountId, capturedTransaction.TargetAccountId);
        Assert.Equal(command.TranferTypeId, capturedTransaction.TranferTypeId);
        Assert.Equal(command.Value, capturedTransaction.Value);
        Assert.Equal("pending", capturedTransaction.Status);
        Assert.NotEqual(default, capturedTransaction.CreatedAt);
        Assert.NotEqual(Guid.Empty, capturedTransaction.TransactionExternalId);
    }

    [Fact]
    public async Task Handle_ShouldSaveTransactionInRepository()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TranferTypeId = 1,
            Value = 100.50m
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishMessageOnConfiguredTopic()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TranferTypeId = 1,
            Value = 100.50m
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockMessageProducer.Verify(p => p.ProduceAsync(
                _kafkaTopic,
                It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishMessageWithCorrectData()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TranferTypeId = 1,
            Value = 100.50m
        };

        object capturedEventData = null;
        _mockMessageProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Callback<string, object>((topic, data) => capturedEventData = data)
            .Returns(Task.CompletedTask);

        // Act
        _ = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedEventData);

        var props = capturedEventData.GetType().GetProperties();
        Assert.Contains(props, p => p.Name == "TransactionExternalId");
        Assert.Contains(props, p => p.Name == "Value");
        Assert.Contains(props, p => p.Name == "CreatedAt");
        Assert.Contains(props, p => p.Name == "SourceAccountId");

        var sourceAccountId = capturedEventData.GetType().GetProperty("SourceAccountId")?.GetValue(capturedEventData);
        Assert.Equal(command.SourceAccountId, sourceAccountId);
    }

    [Fact]
    public async Task Handle_ShouldReturnTransactionExternalId()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TranferTypeId = 1,
            Value = 100.50m
        };

        Transaction? savedTransaction = null;
        _mockRepository
            .Setup(r => r.SaveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Callback<Transaction, CancellationToken>((t, ct) => savedTransaction = t)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        if (savedTransaction != null) Assert.Equal(savedTransaction.TransactionExternalId, result);
    }
}