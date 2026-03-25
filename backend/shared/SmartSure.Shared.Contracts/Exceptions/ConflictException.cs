namespace SmartSure.Shared.Contracts.Exceptions;

/// <summary>
/// Thrown when a business rule conflict occurs (HTTP 409).
/// </summary>
public class ConflictException : SmartSureException
{
    public ConflictException(string message)
        : base(message, 409) { }
}
