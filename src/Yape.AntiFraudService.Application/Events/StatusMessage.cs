namespace Yape.AntiFraudService.Application.Events;

public record StatusMessage(Guid TransactionExternalId, string Status);