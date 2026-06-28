using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Zyfy.Internal;
using Zyfy.Tests.Helpers;

namespace Zyfy.Tests;

public sealed class HttpTransportTests
{
    private const string TestKey = "ea_live_testkey";

    private static (HttpTransport transport, MockHttpHandler handler) MakeTransport(bool debug = false)
    {
        var handler = new MockHttpHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = null };
        var options = new ZyfyOptions
        {
            ApiKey = TestKey,
            HttpClient = httpClient,
            Debug = debug,
        };
        var transport = new HttpTransport(options);
        transport.SleepAsync = (_, __) => Task.CompletedTask;
        return (transport, handler);
    }

    private static string VehicleJson(bool enrichmentPending = false) =>
        $@"{{""registration"":""AB12CDE"",""make"":""Ford"",""model"":""Focus"",""vehicleType"":""car"",
""colour"":null,""fuelType"":null,""engineCapacityCc"":null,""yearOfManufacture"":null,
""monthOfFirstRegistration"":null,""vehicleAgeYears"":null,""summary"":null,""signals"":null,
""scores"":null,""fleetFailureProfile"":null,""fleetAdvisoryProfile"":null,
""sources"":{{""motHistory"":""DVSA"",""mutableData"":""DVLA"",""safetyRating"":null}},
""schemaVersion"":""1"",""enrichmentPending"":{enrichmentPending.ToString().ToLower()},
""dataAsOf"":""2026-01-01T00:00:00Z"",""checkedAt"":""2026-01-01T00:00:00Z""}}";

    [Fact]
    public async Task SendAsync_SetsZyfySourceHeader()
    {
        var (transport, handler) = MakeTransport();
        handler.EnqueueOk(VehicleJson());

        await transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE");

        Assert.Single(handler.Requests);
        var req = handler.Requests[0];
        Assert.True(req.Headers.TryGetValues("X-Zyfy-Source", out var vals));
        Assert.Equal("zyfy-dotnet", Assert.Single(vals));
    }

    [Fact]
    public async Task SendAsync_SetsXApiKeyHeader()
    {
        var (transport, handler) = MakeTransport();
        handler.EnqueueOk(VehicleJson());

        await transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE");

        var req = handler.Requests[0];
        Assert.True(req.Headers.TryGetValues("X-Api-Key", out var vals));
        Assert.Equal(TestKey, Assert.Single(vals));
    }

    [Fact]
    public async Task SendAsync_DebugMode_RedactsApiKeyInOutput()
    {
        var (transport, handler) = MakeTransport(debug: true);
        handler.EnqueueOk(VehicleJson());

        var origErr = Console.Error;
        using var sw = new StringWriter();
        Console.SetError(sw);
        try
        {
            await transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE");
        }
        finally
        {
            Console.SetError(origErr);
        }

        var output = sw.ToString();
        Assert.Contains("ea_live_***", output);
        Assert.DoesNotContain(TestKey, output);
    }

    [Fact]
    public async Task SendAsync_PopulatesQuotaFromHeaders()
    {
        var (transport, handler) = MakeTransport();
        handler.EnqueueOk(VehicleJson());

        var result = await transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE");

        Assert.NotNull(result.Quota);
        Assert.Equal(1000L, result.Quota!.Limit);
        Assert.Equal(42L, result.Quota!.Used);
        Assert.Equal(958L, result.Quota!.Remaining);
        Assert.Equal(1100L, result.Quota!.GraceLimit);
        Assert.Equal("2026-07-01T00:00:00Z", result.Quota!.Resets);
    }

    [Fact]
    public async Task SendAsync_UnlimitedQuota_LimitIsNull()
    {
        var (transport, handler) = MakeTransport();
        handler.EnqueueOk(VehicleJson(), new Dictionary<string, string>
        {
            ["X-Quota-Limit"] = "unlimited",
            ["X-Quota-Remaining"] = "unlimited",
        });

        var result = await transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE");

        Assert.NotNull(result.Quota);
        Assert.Null(result.Quota!.Limit);
        Assert.Null(result.Quota!.Remaining);
    }

    [Fact]
    public async Task SendAsync_401_ThrowsAuthenticationException()
    {
        var (transport, handler) = MakeTransport();
        handler.Enqueue(HttpStatusCode.Unauthorized, @"{""error"":""Invalid API key""}");

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE"));
    }

    [Fact]
    public async Task SendAsync_404_ThrowsNotFoundException()
    {
        var (transport, handler) = MakeTransport();
        handler.Enqueue(HttpStatusCode.NotFound, @"{""error"":""Not found""}");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE"));
    }

    [Fact]
    public async Task SendAsync_422_ThrowsValidationExceptionWithCode()
    {
        var (transport, handler) = MakeTransport();
        handler.Enqueue(HttpStatusCode.UnprocessableEntity, @"{""error"":""Unsupported region"",""code"":""unsupported_region""}");

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE"));

        Assert.Equal("unsupported_region", ex.Code);
    }

    [Fact]
    public async Task SendAsync_429RateLimit_ThrowsRateLimitException()
    {
        var (transport, handler) = MakeTransport();
        handler.Enqueue(HttpStatusCode.TooManyRequests, @"{""error"":""Rate limit"",""code"":""rate_limit""}",
            new Dictionary<string, string> { ["Retry-After"] = "30" });

        var ex = await Assert.ThrowsAsync<RateLimitException>(() =>
            transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE"));

        Assert.Equal(30, ex.RetryAfter);
    }

    [Fact]
    public async Task SendAsync_429QuotaExhausted_ThrowsQuotaExhaustedException()
    {
        var (transport, handler) = MakeTransport();
        handler.Enqueue(HttpStatusCode.TooManyRequests,
            @"{""error"":""Quota exhausted"",""code"":""quota_exhausted"",""resets"":""2026-07-01T00:00:00Z""}",
            new Dictionary<string, string> { ["Retry-After"] = "86400" });

        var ex = await Assert.ThrowsAsync<QuotaExhaustedException>(() =>
            transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE"));

        Assert.Equal(86400, ex.RetryAfter);
        Assert.Equal("2026-07-01T00:00:00Z", ex.Resets);
    }

    [Fact]
    public async Task SendAsync_5xx_ThrowsApiException()
    {
        var (transport, handler) = MakeTransport();
        handler.Enqueue(HttpStatusCode.InternalServerError, @"{""error"":""Internal error""}");

        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE"));

        Assert.Equal(500, ex.StatusCode);
    }

    [Fact]
    public async Task SendAsync_NetworkError_ThrowsNetworkException()
    {
        var options = new ZyfyOptions
        {
            ApiKey = TestKey,
            HttpClient = new HttpClient(new ThrowingHandler()),
        };
        var transport = new HttpTransport(options);

        await Assert.ThrowsAsync<NetworkException>(() =>
            transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE"));
    }

    [Fact]
    public async Task SendAsync_MaxEnrichmentRetriesZero_ReturnsImmediatelyWithPending()
    {
        var (transport, handler) = MakeTransport();
        handler.EnqueueOk(VehicleJson(enrichmentPending: true));

        var sleptSeconds = 0;
        transport.SleepAsync = (s, _) =>
        {
            sleptSeconds += s;
            return Task.CompletedTask;
        };

        var result = await transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE", maxEnrichmentRetries: 0);

        Assert.True(result.EnrichmentPending);
        Assert.Equal(0, sleptSeconds);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task SendAsync_Retries_ExitsEarlyWhenEnrichmentPendingFalse()
    {
        var (transport, handler) = MakeTransport();

        // First response: enrichmentPending=true with Retry-After
        handler.Enqueue(HttpStatusCode.OK, VehicleJson(enrichmentPending: true),
            new Dictionary<string, string>
            {
                ["Retry-After"] = "5",
                ["X-Quota-Limit"] = "1000",
                ["X-Quota-Used"] = "1",
                ["X-Quota-Remaining"] = "999",
            });

        // Second response: enrichmentPending=false
        handler.EnqueueOk(VehicleJson(enrichmentPending: false));

        var sleepCount = 0;
        transport.SleepAsync = (_, __) =>
        {
            sleepCount++;
            return Task.CompletedTask;
        };

        var result = await transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE", maxEnrichmentRetries: 3);

        Assert.False(result.EnrichmentPending);
        Assert.Equal(1, sleepCount);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_RetriesExhausted_ReturnsLastResponseWithoutThrowing()
    {
        var (transport, handler) = MakeTransport();

        handler.EnqueueOk(VehicleJson(enrichmentPending: true));
        handler.EnqueueOk(VehicleJson(enrichmentPending: true));
        handler.EnqueueOk(VehicleJson(enrichmentPending: true));

        transport.SleepAsync = (_, __) => Task.CompletedTask;

        var result = await transport.GetAsync<Zyfy.Models.VehicleResult>("/vehicle/AB12CDE", maxEnrichmentRetries: 2);

        Assert.True(result.EnrichmentPending);
        Assert.Equal(3, handler.Requests.Count);
    }

    // A handler that always throws HttpRequestException
    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("connection refused");
        }
    }
}
