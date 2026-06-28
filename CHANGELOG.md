# Changelog

## [0.1.0] — 2026-06-27

Initial release.

- Vehicle lookup, synchronous bulk lookup, and async bulk job submission/polling/deletion
- Postcode lookup, nearest, within, synchronous bulk lookup, and async bulk jobs
- Sync and async methods on all resources (`Lookup`/`LookupAsync`, `BulkLookup`/`BulkLookupAsync`, etc.)
- Enrichment retry with configurable `MaxEnrichmentRetries` (default 10)
- Full typed `record` response objects for all endpoints
- `Quota` object on every successful response, populated from response headers
- Debug mode: log requests and responses to `Console.Error` with API key redacted
- `ZyfyException` hierarchy: `AuthenticationException`, `NotFoundException`, `ValidationException`, `RateLimitException`, `QuotaExhaustedException`, `ApiException`, `NetworkException`
- `ZyfyOptions` for full client configuration including optional external `HttpClient` injection
- Targets `netstandard2.0` — compatible with .NET Framework 4.6.1+, .NET 6+, and VB.NET
