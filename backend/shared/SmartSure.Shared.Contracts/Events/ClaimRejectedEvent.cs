namespace SmartSure.Shared.Contracts.Events;

public record ClaimRejectedEvent(
    Guid ClaimId,
    Guid AdminId,
    string Reason,
    DateTime RejectedAt,
    string UserEmail = "",
    string UserName = ""
);
