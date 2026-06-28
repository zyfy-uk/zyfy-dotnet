using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Zyfy.Models;

namespace Zyfy.Internal;

internal sealed class HttpTransport : IDisposable
{
    private const string SourceHeader = "zyfy-dotnet";
    private const string DefaultBaseUrl = "https://zyfy.uk/v1";
    private const int DefaultTimeoutMs = 10_000;

    internal static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new QuotaValueConverter() },
    };

    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly string _apiKey;
    private readonly int _maxEnrichmentRetries;
    private readonly string _baseUrl;
    private readonly bool _debug;
    private readonly string _sourceHeaderValue;

    // Replaced in tests to avoid real Task.Delay
    internal Func<int, CancellationToken, Task> SleepAsync = (seconds, ct) =>
        Task.Delay(TimeSpan.FromSeconds(seconds), ct);

    internal HttpTransport(ZyfyOptions options)
    {
        _apiKey = ResolveApiKey(options.ApiKey);
        _maxEnrichmentRetries = options.MaxEnrichmentRetries;
        _baseUrl = (options.BaseUrl ?? DefaultBaseUrl).TrimEnd('/');
        _debug = options.Debug;
        _sourceHeaderValue = options.Source is not null ? $"{SourceHeader} {options.Source}" : SourceHeader;

        if (options.HttpClient is not null)
        {
            _httpClient = options.HttpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(options.TimeoutMs),
            };
            _ownsHttpClient = true;
        }
    }

    internal async Task<T> SendAsync<T>(
        string method,
        string path,
        object? body,
        int? maxEnrichmentRetries,
        CancellationToken cancellationToken) where T : class
    {
        var maxRetries = maxEnrichmentRetries ?? _maxEnrichmentRetries;
        var url = _baseUrl + path;
        var attempt = 0;

        while (true)
        {
            if (_debug)
            {
                Console.Error.WriteLine($"[zyfy] → {method} {url} (X-Api-Key: {RedactKey()}, attempt {attempt + 1})");
            }

            var request = BuildRequest(method, url, body);
            HttpResponseMessage response;

            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new NetworkException($"Request timed out after {DefaultTimeoutMs}ms", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new NetworkException("Request was cancelled.", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new NetworkException($"Network error: {ex.Message}", ex);
            }

            var retryAfterHeader = GetRetryAfter(response);
            var rawBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (_debug)
            {
                var remaining = GetHeader(response, "X-Quota-Remaining") ?? "n/a";
                Console.Error.WriteLine($"[zyfy] ← {(int)response.StatusCode} (quota-remaining: {remaining}, retry-after: {retryAfterHeader?.ToString() ?? "none"})");
            }

            if (!response.IsSuccessStatusCode)
            {
                ThrowForStatus(response, rawBody, retryAfterHeader);
            }

            var quota = ParseQuota(response);

            // Inject quota from headers into the JSON body before deserialising,
            // since quota comes from response headers rather than the body.
            var enrichedJson = InjectQuota(rawBody, quota);
            var data = JsonSerializer.Deserialize<T>(enrichedJson, JsonOptions)!;

            var enrichmentPending = GetEnrichmentPending(rawBody);

            if (enrichmentPending && attempt < maxRetries)
            {
                var delaySecs = retryAfterHeader ?? 5;
                if (_debug)
                {
                    Console.Error.WriteLine($"[zyfy] ↺ enrichmentPending=true, retrying in {delaySecs}s (attempt {attempt + 1}/{maxRetries})");
                }
                await SleepAsync(delaySecs, cancellationToken).ConfigureAwait(false);
                attempt++;
                continue;
            }

            return data;
        }
    }

    internal T Send<T>(
        string method,
        string path,
        object? body = null,
        int? maxEnrichmentRetries = null) where T : class
    {
        return SendAsync<T>(method, path, body, maxEnrichmentRetries, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    internal Task<T> GetAsync<T>(string path, int? maxEnrichmentRetries = null, CancellationToken cancellationToken = default) where T : class
    {
        return SendAsync<T>("GET", path, null, maxEnrichmentRetries, cancellationToken);
    }

    internal T Get<T>(string path, int? maxEnrichmentRetries = null) where T : class
    {
        return Send<T>("GET", path, null, maxEnrichmentRetries);
    }

    internal Task<T> PostAsync<T>(string path, object body, int? maxEnrichmentRetries = null, CancellationToken cancellationToken = default) where T : class
    {
        return SendAsync<T>("POST", path, body, maxEnrichmentRetries, cancellationToken);
    }

    internal T Post<T>(string path, object body, int? maxEnrichmentRetries = null) where T : class
    {
        return Send<T>("POST", path, body, maxEnrichmentRetries);
    }

    internal Task<T> DeleteAsync<T>(string path, CancellationToken cancellationToken = default) where T : class
    {
        return SendAsync<T>("DELETE", path, null, null, cancellationToken);
    }

    internal T Delete<T>(string path) where T : class
    {
        return Send<T>("DELETE", path);
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private HttpRequestMessage BuildRequest(string method, string url, object? body)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        request.Headers.Add("X-Api-Key", _apiKey);
        request.Headers.Add("X-Zyfy-Source", _sourceHeaderValue);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    private static void ThrowForStatus(HttpResponseMessage response, string rawBody, int? retryAfterHeader)
    {
        var status = (int)response.StatusCode;
        string? errorMsg = null;
        string? code = null;
        string? resets = null;
        int? retryAfterBody = null;

        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out var errorEl))
            {
                errorMsg = errorEl.GetString();
            }
            if (root.TryGetProperty("code", out var codeEl))
            {
                code = codeEl.GetString();
            }
            if (root.TryGetProperty("resets", out var resetsEl))
            {
                resets = resetsEl.GetString();
            }
            if (root.TryGetProperty("retryAfterSeconds", out var raEl) && raEl.ValueKind == JsonValueKind.Number)
            {
                retryAfterBody = raEl.GetInt32();
            }
        }
        catch (JsonException)
        {
            // Ignore — leave fields null
        }

        if (status == 401)
        {
            throw new AuthenticationException(errorMsg ?? "Invalid or missing API key", rawBody);
        }

        if (status == 404)
        {
            throw new NotFoundException(errorMsg ?? "Resource not found", rawBody);
        }

        if (status == 422)
        {
            throw new ValidationException(errorMsg ?? "Validation error", code ?? "unknown", rawBody);
        }

        if (status == 429)
        {
            if (code == "quota_exhausted")
            {
                var retryAfter = retryAfterHeader ?? retryAfterBody ?? 0;
                throw new QuotaExhaustedException("Monthly quota exhausted", retryAfter, resets ?? string.Empty, rawBody);
            }

            throw new RateLimitException("Rate limit exceeded", retryAfterHeader ?? 60, rawBody);
        }

        throw new ApiException($"API error {status}", status, rawBody);
    }

    private static Quota ParseQuota(HttpResponseMessage response)
    {
        return new Quota
        {
            Limit = ParseQuotaValue(GetHeader(response, "X-Quota-Limit")),
            Used = ParseLong(GetHeader(response, "X-Quota-Used")),
            Remaining = ParseQuotaValue(GetHeader(response, "X-Quota-Remaining")),
            GraceLimit = ParseNullableLong(GetHeader(response, "X-Quota-Grace-Limit")),
            Resets = GetHeader(response, "X-Quota-Resets"),
        };
    }

    private static string InjectQuota(string rawBody, Quota quota)
    {
        var trimmed = rawBody.TrimEnd();
        if (!trimmed.EndsWith("}"))
        {
            return rawBody;
        }

        var quotaJson = JsonSerializer.Serialize(quota, JsonOptions);
        return trimmed.Substring(0, trimmed.Length - 1) + ",\"quota\":" + quotaJson + "}";
    }

    private static long? ParseQuotaValue(string? raw)
    {
        if (raw is null)
        {
            return null;
        }
        if (raw == "unlimited")
        {
            return null;
        }
        if (long.TryParse(raw, out var val))
        {
            return val;
        }
        return null;
    }

    private static long ParseLong(string? raw)
    {
        if (raw is not null && long.TryParse(raw, out var val))
        {
            return val;
        }
        return 0;
    }

    private static long? ParseNullableLong(string? raw)
    {
        if (raw is not null && long.TryParse(raw, out var val))
        {
            return val;
        }
        return null;
    }

    private static string? GetHeader(HttpResponseMessage response, string name)
    {
        if (response.Headers.TryGetValues(name, out var vals))
        {
            foreach (var v in vals)
            {
                return v;
            }
        }
        return null;
    }

    private static int? GetRetryAfter(HttpResponseMessage response)
    {
        var raw = GetHeader(response, "Retry-After");
        if (raw is not null && int.TryParse(raw, out var val))
        {
            return val;
        }
        return null;
    }

    private static bool GetEnrichmentPending(string rawBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            if (doc.RootElement.TryGetProperty("enrichmentPending", out var el))
            {
                return el.ValueKind == JsonValueKind.True;
            }
        }
        catch (JsonException)
        {
            // Ignore
        }
        return false;
    }

    private string RedactKey()
    {
        if (_apiKey.StartsWith("ea_live_", StringComparison.Ordinal))
        {
            return "ea_live_***";
        }
        if (_apiKey.StartsWith("ea_app_", StringComparison.Ordinal))
        {
            return "ea_app_***";
        }
        if (_apiKey.Length > 6)
        {
            return _apiKey.Substring(0, 6) + "***";
        }
        return "***";
    }

    private static string ResolveApiKey(string? explicitKey)
    {
        var key = explicitKey ?? Environment.GetEnvironmentVariable("ZYFY_API_KEY");
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Zyfy API key is required. Pass ApiKey in ZyfyOptions or set the ZYFY_API_KEY environment variable.");
        }
        return key;
    }
}
