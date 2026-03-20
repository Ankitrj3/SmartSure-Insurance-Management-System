namespace SmartSure.Shared.Contracts.Events;

public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FullName,
    string PhoneNumber,
    DateTime RegisteredAt,
    bool IsGoogleAuth = false
);
