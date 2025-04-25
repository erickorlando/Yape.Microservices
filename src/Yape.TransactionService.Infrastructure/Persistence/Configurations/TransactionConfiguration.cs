using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yape.TransactionService.Domain.Entities;

namespace Yape.TransactionService.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(p => p.TransactionExternalId);
        
        builder.Property(p => p.Status)
            .IsRequired();

        builder.HasIndex(p => p.Status);
    }
}