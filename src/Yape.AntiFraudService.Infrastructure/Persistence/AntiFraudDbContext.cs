using Microsoft.EntityFrameworkCore;
using Yape.AntiFraudService.Domain.Entities;

namespace Yape.AntiFraudService.Infrastructure.Persistence;

public class AntiFraudDbContext(DbContextOptions<AntiFraudDbContext> options) : DbContext(options)
{
    // DbSet for the AccumulatedValue entity
    public DbSet<AccumulatedValue> AccumulatedValues { get; set; }

    // Add other DbSets if needed for Anti-Fraud related data

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the AccumulatedValue entity
        modelBuilder.Entity<AccumulatedValue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AccountId, e.Date }).IsUnique(); // Ensure only one entry per account per day

            entity.Property(e => e.TotalValue)
                .HasPrecision(18, 2);
        });


        base.OnModelCreating(modelBuilder);
    }
}