namespace SmartSure.Shared.Contracts.Events;

public record PolicyCancelledEvent(
    Guid PolicyId,
    Guid UserId,
    string Reason,
    DateTime CancelledAt
);
