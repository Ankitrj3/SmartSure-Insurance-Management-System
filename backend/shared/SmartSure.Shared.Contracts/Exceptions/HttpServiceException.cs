namespace SmartSure.Shared.Contracts.Exceptions;

/// <summary>
/// Thrown when an HTTP service call fails with an error response.
/// </summary>
public class HttpServiceException : SmartSureException
{
    public HttpServiceException(string message, int statusCode = 500)
        : base(message, statusCode)
    {
    }

    public HttpServiceException(string message, Exception innerException, int statusCode = 500)
        : base(message, innerException, statusCode)
    {
    }
}
