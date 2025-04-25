namespace Yape.AntiFraudService.Domain.Interfaces;

public interface IAccumulatedValueRepository
{
    // Method to get the accumulated value for a specific account on a specific day
    Task<decimal> GetAccumulatedValueForAccountAndDay(Guid accountId, DateTime date, CancellationToken cancellationToken = default);

    // You might also need a method to update the accumulated value after a transaction is approved
    Task UpdateAccumulatedValueForAccountAndDay(Guid accountId, DateTime date, decimal value, CancellationToken cancellationToken = default);
}