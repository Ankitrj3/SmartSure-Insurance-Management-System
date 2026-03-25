namespace SmartSure.Shared.Contracts.Exceptions;

/// <summary>
/// Thrown when the caller is not authorized (HTTP 401).
/// </summary>
public class UnauthorizedException : SmartSureException
{
    public UnauthorizedException(string message = "You are not authorized to perform this action.")
        : base(message, 401) { }
}
