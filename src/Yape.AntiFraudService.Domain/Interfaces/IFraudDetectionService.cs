namespace Yape.AntiFraudService.Domain.Interfaces;

public interface IFraudDetectionService
{
    Task<bool> CheckForFraud(Guid transactionExternalId, decimal transactionValue, 
        Guid sourceAccountId, DateTime transactionDate);
}