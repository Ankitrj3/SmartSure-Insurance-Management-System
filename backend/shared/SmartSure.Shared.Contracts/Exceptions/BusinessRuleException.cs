namespace SmartSure.Shared.Contracts.Exceptions;

/// <summary>
/// Thrown when a domain / business rule is violated (HTTP 422).
/// </summary>
public class BusinessRuleException : SmartSureException
{
    public BusinessRuleException(string message)
        : base(message, 422) { }
}
