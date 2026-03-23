namespace SmartSure.Shared.Contracts.Events;

public record ClaimStatusChangedEvent(
    Guid ClaimId,
    string OldStatus,
    string NewStatus,
    string ChangedBy,
    DateTime ChangedAt,
    Guid UserId = default,
    string? Reason = null
);
