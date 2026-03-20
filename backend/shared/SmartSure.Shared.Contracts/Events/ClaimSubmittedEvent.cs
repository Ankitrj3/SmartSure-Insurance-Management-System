namespace SmartSure.Shared.Contracts.Events;

public record ClaimSubmittedEvent(
    Guid ClaimId,
    Guid PolicyId,
    Guid UserId,
    string Description,
    DateTime SubmittedAt
);
