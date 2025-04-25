using MediatR;

namespace Yape.TransactionService.Application.Events;

public class TransactionUpdatedEventMessage : IRequest<Guid>
{
    public Guid TransactionExternalId { get; set; }
    public Guid TargetAccountId { get; set; }
    public string Status { get; set; } = null!;
}