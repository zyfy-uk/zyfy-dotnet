using System.ComponentModel;
using System.Net.Http;

namespace Zyfy;

/// <summary>
/// Configuration options for <see cref="ZyfyClient"/>.
/// </summary>
public sealed class ZyfyOptions
{
    /// <summary>
    /// Zyfy API key. Falls back to the <c>ZYFY_API_KEY</c> environment variable.
    /// Throws at construction if neither is provided.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// Maximum number of automatic retries when the API returns <c>enrichmentPending: true</c>.
    /// Each retry waits the number of seconds specified in the <c>Retry-After</c> response header.
    /// Set to 0 to disable auto-retry entirely.
    /// </summary>
    /// <value>Default: 10</value>
    public int MaxEnrichmentRetries { get; init; } = 10;

    /// <summary>
    /// HTTP request timeout in milliseconds.
    /// </summary>
    /// <value>Default: 10000 (10 seconds)</value>
    public int TimeoutMs { get; init; } = 10_000;

    /// <summary>
    /// Override the API base URL. Useful for testing against a local instance.
    /// </summary>
    /// <value>Default: <c>https://zyfy.uk/v1</c></value>
    public string BaseUrl { get; init; } = "https://zyfy.uk/v1";

    /// <summary>
    /// Log requests and responses to <see cref="System.Console.Error"/>. The API key is always redacted.
    /// </summary>
    /// <value>Default: false</value>
    public bool Debug { get; init; } = false;

    /// <summary>
    /// Optional pre-configured <see cref="HttpClient"/> to use for all requests.
    /// The caller retains ownership and is responsible for disposing it.
    /// When null, an internal <see cref="HttpClient"/> is created and disposed with <see cref="ZyfyClient"/>.
    /// </summary>
    public HttpClient? HttpClient { get; init; }

    /// <summary>Appended to the X-Zyfy-Source header after the library identifier.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string? Source { get; init; }
}
