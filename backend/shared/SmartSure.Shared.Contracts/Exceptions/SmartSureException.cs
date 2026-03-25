namespace SmartSure.Shared.Contracts.Exceptions;

/// <summary>
/// Base custom exception for all SmartSure domain exceptions.
/// </summary>
public abstract class SmartSureException : Exception
{
    public int StatusCode { get; }

    protected SmartSureException(string message, int statusCode = 500)
        : base(message)
    {
        StatusCode = statusCode;
    }

    protected SmartSureException(string message, Exception innerException, int statusCode = 500)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
