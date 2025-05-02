using Microsoft.Extensions.Logging;
using Moq;
using Yape.AntiFraudService.Domain.Interfaces;
using Yape.AntiFraudService.Domain.Services;

namespace Yape.AntiFraudService.UnitTests;

public class FraudDetectionUnitTests
{
    [Fact]
    public async Task CheckForFraud_ReturnsTrue_WhenTransactionValueExceeds2000()
    {
        // Arrange
        var transactionExternalId = Guid.NewGuid();
        var transactionValue = 2500m;
        var sourceAccountId = Guid.NewGuid();
        var transactionDate = DateTime.UtcNow;

        var accumulatedValueRepositoryMock = new Mock<IAccumulatedValueRepository>();
        var loggerMock = new Mock<ILogger<FraudDetectionService>>();

        var service = new FraudDetectionService(accumulatedValueRepositoryMock.Object, loggerMock.Object);

        // Act
        var result =
            await service.CheckForFraud(transactionExternalId, transactionValue, sourceAccountId, transactionDate);

        // Assert
        Assert.True(result);
        
    }

    [Fact]
    public async Task CheckForFraud_ReturnsTrue_WhenDailyAccumulatedValueExceeds20000()
    {
        // Arrange
        var transactionExternalId = Guid.NewGuid();
        var transactionValue = 15000m;
        var sourceAccountId = Guid.NewGuid();
        var transactionDate = DateTime.UtcNow;

        var accumulatedValueRepositoryMock = new Mock<IAccumulatedValueRepository>();
        accumulatedValueRepositoryMock
            .Setup(x => x.GetAccumulatedValueForAccountAndDay(sourceAccountId, transactionDate.Date,
                CancellationToken.None))
            .ReturnsAsync(10000m);

        var loggerMock = new Mock<ILogger<FraudDetectionService>>();

        var service = new FraudDetectionService(accumulatedValueRepositoryMock.Object, loggerMock.Object);

        // Act
        var result =
            await service.CheckForFraud(transactionExternalId, transactionValue, sourceAccountId, transactionDate);

        // Assert
        Assert.True(result);
        
    }

    [Fact]
    public async Task CheckForFraud_ReturnsFalse_WhenNoFraudDetected()
    {
        // Arrange
        var transactionExternalId = Guid.NewGuid();
        var transactionValue = 1000m;
        var sourceAccountId = Guid.NewGuid();
        var transactionDate = DateTime.UtcNow;

        var accumulatedValueRepositoryMock = new Mock<IAccumulatedValueRepository>();
        accumulatedValueRepositoryMock
            .Setup(x => x.GetAccumulatedValueForAccountAndDay(sourceAccountId, transactionDate.Date, 
                CancellationToken.None))
            .ReturnsAsync(5000m);

        var loggerMock = new Mock<ILogger<FraudDetectionService>>();

        var service = new FraudDetectionService(accumulatedValueRepositoryMock.Object, loggerMock.Object);

        // Act
        var result =
            await service.CheckForFraud(transactionExternalId, transactionValue, sourceAccountId, transactionDate);

        // Assert
        Assert.False(result);
       
    }
}