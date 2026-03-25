namespace SmartSure.Shared.Contracts.Exceptions;

/// <summary>
/// Thrown when a requested resource is not found (HTTP 404).
/// </summary>
public class NotFoundException : SmartSureException
{
    public NotFoundException(string resource)
        : base($"{resource} was not found.", 404) { }

    public NotFoundException(string resource, Guid id)
        : base($"{resource} with ID '{id}' was not found.", 404) { }
}
