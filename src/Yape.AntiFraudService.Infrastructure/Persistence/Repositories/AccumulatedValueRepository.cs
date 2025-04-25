using Microsoft.EntityFrameworkCore;
using Yape.AntiFraudService.Domain.Entities;
using Yape.AntiFraudService.Domain.Interfaces;

namespace Yape.AntiFraudService.Infrastructure.Persistence.Repositories;

public class AccumulatedValueRepository : IAccumulatedValueRepository
    {
        private readonly AntiFraudDbContext _dbContext;

        public AccumulatedValueRepository(AntiFraudDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<decimal> GetAccumulatedValueForAccountAndDay(Guid accountId, DateTime date, CancellationToken cancellationToken = default)
        {
            // Ensure we only consider the date part
            var dateOnly = date.Date;

            // Query the database for the accumulated value for the given account and date
            var accumulatedValueEntity = await _dbContext.AccumulatedValues
                .AsNoTracking() // 
                .FirstOrDefaultAsync(av => av.AccountId == accountId && av.Date == dateOnly, cancellationToken);

            // Return the TotalValue if found, otherwise return 0
            return accumulatedValueEntity?.TotalValue ?? 0;
        }

        public async Task UpdateAccumulatedValueForAccountAndDay(Guid accountId, DateTime date, decimal valueToAdd, CancellationToken cancellationToken = default)
        {
             // Ensure we only consider the date part
            var dateOnly = date.Date;

            // Find the existing entry or create a new one
            var accumulatedValueEntity = await _dbContext.AccumulatedValues
                .FirstOrDefaultAsync(av => av.AccountId == accountId && av.Date == dateOnly,
                    cancellationToken);

            if (accumulatedValueEntity == null)
            {
                // Create a new entry if it doesn't exist
                accumulatedValueEntity = new AccumulatedValue
                {
                    AccountId = accountId,
                    Date = dateOnly,
                    TotalValue = valueToAdd
                };
                _dbContext.AccumulatedValues.Add(accumulatedValueEntity);
            }
            else
            {
                // Update the existing entry
                accumulatedValueEntity.TotalValue += valueToAdd;
            }

            await _dbContext.SaveChangesAsync(cancellationToken); // Save changes to the database
        }
    }