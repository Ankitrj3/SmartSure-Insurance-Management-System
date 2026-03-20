namespace SmartSure.Shared.Contracts.Events;

public record PolicyActivatedEvent(
    Guid PolicyId,
    Guid UserId,
    Guid TypeId,
    Guid SubTypeId,
    DateTime ActivatedAt
);
