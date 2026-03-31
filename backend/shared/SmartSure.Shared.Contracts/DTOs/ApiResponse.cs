namespace SmartSure.Shared.Contracts.DTOs;

/// <summary>
/// Represent or implements ApiResponse.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Performs the Ok operation.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    /// <summary>
    /// Performs the Fail operation.
    /// </summary>
    public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

/// <summary>
/// Represent or implements ApiResponse.
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Performs the Ok operation.
    /// </summary>
    public static ApiResponse Ok(string? message = null) =>
        new() { Success = true, Message = message };

    /// <summary>
    /// Performs the Fail operation.
    /// </summary>
    public static ApiResponse Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}
