namespace SmartSure.Shared.Contracts.Events;

public record ClaimApprovedEvent(
    Guid ClaimId,
    Guid AdminId,
    string Notes,
    DateTime ApprovedAt,
    string UserEmail = "",
    string UserName = ""
);
