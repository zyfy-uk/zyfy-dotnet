# Zyfy .NET

Official .NET client library for the [Zyfy](https://zyfy.uk) UK data enrichment API — vehicle intelligence, postcode intelligence, and more.

Get your API key at [zyfy.uk/signup](https://zyfy.uk/signup). Full API docs at [zyfy.uk/docs](https://zyfy.uk/docs).

## Installation

```
dotnet add package Zyfy
```

## Quickstart

```csharp
using Zyfy;

// API key from constructor, or set ZYFY_API_KEY environment variable
var client = new ZyfyClient("ea_live_...");

// Vehicle lookup
var vehicle = await client.Vehicle.LookupAsync("AB12CDE");
Console.WriteLine($"{vehicle.Make} {vehicle.Model}");
Console.WriteLine($"Tax: {vehicle.Signals?.TaxStatus}");
Console.WriteLine($"Quota remaining: {vehicle.Quota.Remaining}");

// Postcode lookup
var postcode = await client.Postcode.LookupAsync("SW1A 2AA");
Console.WriteLine($"{postcode.AdminDistrict}");
Console.WriteLine($"Crime band: {postcode.Signals?.Crime?.RateBand}");
```

## Async usage

All methods have both async (`*Async`) and sync variants:

```csharp
// Async (preferred)
var vehicle = await client.Vehicle.LookupAsync("AB12CDE");

// Sync
var vehicle = client.Vehicle.Lookup("AB12CDE");
```

Async methods accept an optional `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var vehicle = await client.Vehicle.LookupAsync("AB12CDE", cancellationToken: cts.Token);
```

## Configuration reference

Pass a `ZyfyOptions` object to `ZyfyClient` to customise behaviour:

```csharp
var client = new ZyfyClient(new ZyfyOptions
{
    ApiKey = "ea_live_...",
    MaxEnrichmentRetries = 5,
    TimeoutMs = 15_000,
    Debug = true,
});
```

| Option | Type | Default | Description |
|---|---|---|---|
| `ApiKey` | `string?` | `null` | Zyfy API key. Falls back to `ZYFY_API_KEY` env var. Throws if neither is set. |
| `MaxEnrichmentRetries` | `int` | `10` | Max retries when `EnrichmentPending` is true. 0 disables auto-retry. |
| `TimeoutMs` | `int` | `10000` | Request timeout in milliseconds. |
| `BaseUrl` | `string` | `https://zyfy.uk/v1` | Override the API base URL (e.g. for local testing). |
| `Debug` | `bool` | `false` | Log requests and responses to `Console.Error`. API key is always redacted. |
| `HttpClient` | `HttpClient?` | `null` | Provide a pre-configured `HttpClient`. Caller owns it and is responsible for disposal. |

## Method reference

### `client.Vehicle`

| Method | Sync | Description |
|---|---|---|
| `LookupAsync(registration, maxEnrichmentRetries?, ct?)` | `Lookup(registration, maxEnrichmentRetries?)` | Look up a single vehicle by UK registration mark. |
| `BulkLookupAsync(registrations[], maxEnrichmentRetries?, ct?)` | `BulkLookup(registrations[], maxEnrichmentRetries?)` | Synchronous bulk lookup (up to your tier's bulk cap). |
| `SubmitBulkAsync(registrations[], ct?)` | `SubmitBulk(registrations[])` | Submit an async bulk job. Returns a `BulkJobSubmitted` with a `JobId`. |
| `GetJobAsync(jobId, ct?)` | `GetJob(jobId)` | Poll an async bulk job. Check `Status == "complete"` before reading `Results`. |
| `DeleteJobAsync(jobId, ct?)` | `DeleteJob(jobId)` | Delete a bulk job and its results. |

### `client.Postcode`

| Method | Sync | Description |
|---|---|---|
| `LookupAsync(postcode, maxEnrichmentRetries?, ct?)` | `Lookup(postcode, maxEnrichmentRetries?)` | Look up a single UK postcode. |
| `NearestAsync(lat, lon, radius?, maxEnrichmentRetries?, ct?)` | `Nearest(lat, lon, radius?, maxEnrichmentRetries?)` | Find the nearest postcode to a WGS84 coordinate. |
| `WithinAsync(lat, lon, radius?, maxEnrichmentRetries?, ct?)` | `Within(lat, lon, radius?, maxEnrichmentRetries?)` | All postcodes within a radius (Starter+ tier). |
| `BulkLookupAsync(postcodes[], maxEnrichmentRetries?, ct?)` | `BulkLookup(postcodes[], maxEnrichmentRetries?)` | Synchronous bulk lookup. |
| `SubmitBulkAsync(postcodes[], ct?)` | `SubmitBulk(postcodes[])` | Submit an async bulk job. |
| `GetJobAsync(jobId, ct?)` | `GetJob(jobId)` | Poll an async bulk job. |
| `DeleteJobAsync(jobId, ct?)` | `DeleteJob(jobId)` | Delete a bulk job. |

## Bulk item types

Bulk results contain items that are either successful or an error. Use pattern matching:

```csharp
var result = await client.Vehicle.BulkLookupAsync(new[] { "AB12CDE", "INVALID" });
foreach (var item in result.Results)
{
    if (item is VehicleResultItem success)
    {
        Console.WriteLine($"{success.Result.Registration}: {success.Result.Make}");
    }
    else if (item is VehicleBulkItemError error)
    {
        Console.WriteLine($"{error.Registration}: {error.Error}");
    }
}
```

## Error handling

All errors extend `ZyfyException`. Catch what you need:

```csharp
try
{
    var vehicle = await client.Vehicle.LookupAsync("AB12CDE");
}
catch (QuotaExhaustedException ex)
{
    // Monthly quota exhausted (HTTP 429).
    // ex.Resets is an ISO 8601 UTC string indicating when the quota rolls over.
    // It is null when no monthly reset applies — always null-check.
    if (ex.Resets is not null)
    {
        var resetsAt = DateTimeOffset.Parse(ex.Resets);
        var hoursUntilReset = (int)Math.Round((resetsAt - DateTimeOffset.UtcNow).TotalHours);
        Console.Error.WriteLine($"Quota exhausted. Resets at {resetsAt:R} (~{hoursUntilReset}h)");
    }
    else
    {
        Console.Error.WriteLine("Quota exhausted. Contact support to increase your limit.");
    }
}
catch (RateLimitException ex)
{
    // Per-minute rate limit exceeded (HTTP 429). Back off by ex.RetryAfter seconds and retry.
    Console.Error.WriteLine($"Rate limited. Retrying in {ex.RetryAfter}s");
    await Task.Delay(TimeSpan.FromSeconds(ex.RetryAfter));
    // retry the call...
}
catch (AuthenticationException)
{
    // Invalid or missing API key (HTTP 401)
}
catch (NotFoundException)
{
    // Vehicle or postcode not found (HTTP 404)
}
catch (ValidationException ex)
{
    // Input validation error (HTTP 422). ex.Code identifies the specific problem.
    // Common codes: "unsupported_region" (BT postcodes), "invalid_format"
    Console.Error.WriteLine($"Validation error: {ex.Code}");
}
catch (ApiException ex)
{
    // Unexpected server error (HTTP 5xx). ex.StatusCode contains the HTTP status.
}
catch (NetworkException)
{
    // Connection failure or timeout.
}
catch (ZyfyException)
{
    // Catch-all for any Zyfy error.
}
```

| Exception | HTTP status | When |
|---|---|---|
| `AuthenticationException` | 401 | Invalid or missing API key |
| `NotFoundException` | 404 | Vehicle or postcode not found |
| `ValidationException` | 422 | Invalid input; check `ex.Code` (e.g. `"unsupported_region"`, `"invalid_format"`) |
| `RateLimitException` | 429 | Per-minute rate limit exceeded; `ex.RetryAfter` seconds until safe to retry |
| `QuotaExhaustedException` | 429 | Monthly quota exhausted; `ex.RetryAfter` seconds until reset, `ex.Resets` ISO 8601 datetime (null if not applicable) |
| `ApiException` | 5xx | Server error; `ex.StatusCode` |
| `NetworkException` | — | Connection failure or timeout |

All exception classes expose `RawBody: string` with the full response body for debugging.

## Quota

Every successful response includes a `Quota` property populated from response headers:

```csharp
var vehicle = await client.Vehicle.LookupAsync("AB12CDE");
var quota = vehicle.Quota;

Console.WriteLine($"Limit     : {quota.Limit?.ToString() ?? "unlimited"}");
Console.WriteLine($"Used      : {quota.Used}");
Console.WriteLine($"Remaining : {quota.Remaining?.ToString() ?? "unlimited"}");
Console.WriteLine($"Grace cap : {quota.GraceLimit?.ToString() ?? "n/a"}");
Console.WriteLine($"Resets    : {quota.Resets ?? "n/a"}");
```

`Limit` and `Remaining` are `long?` — `null` means the plan has no monthly cap.

`Resets` is `null` when no monthly cap is in effect. Always null-check before parsing or displaying it:

```csharp
if (quota.Resets is not null)
{
    var resetsAt = DateTimeOffset.Parse(quota.Resets);
    Console.WriteLine($"Quota resets on {resetsAt:D}");
}
```

`GraceLimit` is a small buffer above `Limit` (approximately 10%) that allows a few extra requests before the hard block kicks in. Once `GraceLimit` is exhausted, all requests throw `QuotaExhaustedException` until the quota resets.

To avoid hitting the limit unexpectedly in high-volume applications, read `quota.Remaining` after each response and back off before it reaches zero.

## Enrichment retries

Vehicle lookups may return `EnrichmentPending = true` when background enrichment is still running — typically the first time a registration is seen, or when the vehicle's data is being refreshed. When pending, `Signals`, `Summary`, and `Scores` may be `null` or incomplete. Postcode lookups are always served from a pre-loaded dataset and never set `EnrichmentPending`.

**Automatic retries (default)**

The client retries automatically up to `MaxEnrichmentRetries` times (default: 10), waiting the number of seconds in the `Retry-After` response header (typically 5 seconds) between each attempt. The final result is returned once enrichment completes or retries are exhausted — no exception is thrown either way.

Override the retry limit per call:

```csharp
// Retry up to 3 times instead of the global default
var result = await client.Vehicle.LookupAsync("AB12CDE", maxEnrichmentRetries: 3);
```

**Manual retry pattern**

If you need control over retry timing — for example, in a background job where you prefer to reenqueue rather than block — disable auto-retries and handle `EnrichmentPending` yourself:

```csharp
var client = new ZyfyClient(new ZyfyOptions { MaxEnrichmentRetries = 0 });

var result = await client.Vehicle.LookupAsync("AB12CDE");

if (result.EnrichmentPending)
{
    // Partial data returned — Signals/Summary/Scores may be null.
    // The API will typically have the enriched result ready within 5 seconds.

    // Option A: wait and re-query inline
    await Task.Delay(TimeSpan.FromSeconds(5));
    result = await client.Vehicle.LookupAsync("AB12CDE");

    // Option B: in a background job, persist result.Registration and requeue
    // the job after a delay rather than blocking here.
}

// Use whatever is available — EnrichmentPending may still be true
// if enrichment is unusually slow. The data returned is always valid.
if (result.EnrichmentPending)
{
    Console.WriteLine($"Partial data for {result.Registration} — enrichment still in progress");
}

var recommendation = result.Summary?.BuyRecommendation ?? "pending";
Console.WriteLine($"{result.Registration}: {recommendation}");
```

If auto-retries exhaust while `EnrichmentPending` is still `true`, the last partial response is returned without throwing. All returned fields are valid — only fields that depend on enrichment may be `null`.

## Examples

Runnable examples are in the `examples/` directory:

```bash
# Set your API key
export ZYFY_API_KEY=ea_live_...

# Single lookups
dotnet run --project examples/Zyfy.Examples -- vehicle
dotnet run --project examples/Zyfy.Examples -- postcode

# Geographic queries
dotnet run --project examples/Zyfy.Examples -- nearest   # uses LAT/LON env vars
dotnet run --project examples/Zyfy.Examples -- within    # uses LAT/LON/RADIUS env vars

# Bulk lookups
dotnet run --project examples/Zyfy.Examples -- bulk-vehicle
dotnet run --project examples/Zyfy.Examples -- bulk-postcode

# Error handling scenarios
dotnet run --project examples/Zyfy.Examples -- error vehicle-invalid
dotnet run --project examples/Zyfy.Examples -- error postcode-not-found
dotnet run --project examples/Zyfy.Examples -- error postcode-ni
dotnet run --project examples/Zyfy.Examples -- error bad-auth
```

## Versioning

This library follows [Semantic Versioning](https://semver.org/). See [CHANGELOG.md](CHANGELOG.md) for the full history.

## License

[MIT](LICENSE)
