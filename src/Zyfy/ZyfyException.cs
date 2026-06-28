using System;

namespace Zyfy;

/// <summary>Base class for all exceptions thrown by the Zyfy client library.</summary>
public class ZyfyException : Exception
{
    /// <inheritdoc/>
    public ZyfyException(string message) : base(message) { }

    /// <inheritdoc/>
    public ZyfyException(string message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>Thrown when the API key is missing or invalid (HTTP 401).</summary>
public sealed class AuthenticationException : ZyfyException
{
    /// <summary>The raw response body returned by the API.</summary>
    public string RawBody { get; }

    /// <inheritdoc/>
    public AuthenticationException(string message, string rawBody) : base(message)
    {
        RawBody = rawBody;
    }
}

/// <summary>Thrown when the requested resource was not found (HTTP 404).</summary>
public sealed class NotFoundException : ZyfyException
{
    /// <summary>The raw response body returned by the API.</summary>
    public string RawBody { get; }

    /// <inheritdoc/>
    public NotFoundException(string message, string rawBody) : base(message)
    {
        RawBody = rawBody;
    }
}

/// <summary>Thrown when the request failed input validation (HTTP 422).</summary>
public sealed class ValidationException : ZyfyException
{
    /// <summary>Machine-readable error code from the API body (e.g. <c>unsupported_region</c>).</summary>
    public string Code { get; }

    /// <summary>The raw response body returned by the API.</summary>
    public string RawBody { get; }

    /// <inheritdoc/>
    public ValidationException(string message, string code, string rawBody) : base(message)
    {
        Code = code;
        RawBody = rawBody;
    }
}

/// <summary>Thrown when the per-minute rate limit is exceeded (HTTP 429, code: <c>rate_limit</c>).</summary>
public sealed class RateLimitException : ZyfyException
{
    /// <summary>Seconds to wait before retrying.</summary>
    public int RetryAfter { get; }

    /// <summary>The raw response body returned by the API.</summary>
    public string RawBody { get; }

    /// <inheritdoc/>
    public RateLimitException(string message, int retryAfter, string rawBody) : base(message)
    {
        RetryAfter = retryAfter;
        RawBody = rawBody;
    }
}

/// <summary>Thrown when the monthly quota is exhausted (HTTP 429, code: <c>quota_exhausted</c>).</summary>
public sealed class QuotaExhaustedException : ZyfyException
{
    /// <summary>Seconds until the quota resets.</summary>
    public int RetryAfter { get; }

    /// <summary>ISO 8601 datetime when the quota resets.</summary>
    public string Resets { get; }

    /// <summary>The raw response body returned by the API.</summary>
    public string RawBody { get; }

    /// <inheritdoc/>
    public QuotaExhaustedException(string message, int retryAfter, string resets, string rawBody) : base(message)
    {
        RetryAfter = retryAfter;
        Resets = resets;
        RawBody = rawBody;
    }
}

/// <summary>Thrown for unexpected server-side errors (HTTP 5xx).</summary>
public sealed class ApiException : ZyfyException
{
    /// <summary>HTTP status code returned by the API.</summary>
    public int StatusCode { get; }

    /// <summary>The raw response body returned by the API.</summary>
    public string RawBody { get; }

    /// <inheritdoc/>
    public ApiException(string message, int statusCode, string rawBody) : base(message)
    {
        StatusCode = statusCode;
        RawBody = rawBody;
    }
}

/// <summary>Thrown for network-level failures such as timeouts or connection errors.</summary>
public sealed class NetworkException : ZyfyException
{
    /// <inheritdoc/>
    public NetworkException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}
