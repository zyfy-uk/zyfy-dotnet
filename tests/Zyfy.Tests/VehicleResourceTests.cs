using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Zyfy.Tests.Helpers;

namespace Zyfy.Tests;

public sealed class VehicleResourceTests
{
    private const string TestKey = "ea_live_test";

    private static string VehicleJson() =>
        @"{""registration"":""AB12CDE"",""make"":""Ford"",""model"":""Focus"",""vehicleType"":""car"",
""colour"":null,""fuelType"":null,""engineCapacityCc"":null,""yearOfManufacture"":null,
""monthOfFirstRegistration"":null,""vehicleAgeYears"":null,""summary"":null,""signals"":null,
""scores"":null,""fleetFailureProfile"":null,""fleetAdvisoryProfile"":null,
""sources"":{""motHistory"":""DVSA"",""mutableData"":""DVLA"",""safetyRating"":null},
""schemaVersion"":""1"",""enrichmentPending"":false,
""dataAsOf"":""2026-01-01T00:00:00Z"",""checkedAt"":""2026-01-01T00:00:00Z""}";

    private static string BulkVehicleJson() =>
        @"{""total"":1,""results"":[" + VehicleJson() + "]}";

    private static string JobJson(string jobId) =>
        $@"{{""jobId"":""{jobId}"",""status"":""queued"",""total"":1,""pollUrl"":""/vehicle/bulk/jobs/{jobId}""}}";

    private static string JobStatusJson(string jobId) =>
        $@"{{""jobId"":""{jobId}"",""status"":""complete"",""total"":1,""done"":1,
""createdAt"":""2026-01-01T00:00:00Z"",""completedAt"":""2026-01-01T00:01:00Z"",
""expiresAt"":""2026-01-02T00:00:00Z"",""results"":[{VehicleJson()}]}}";

    private static (ZyfyClient client, MockHttpHandler handler) MakeClient()
    {
        var handler = new MockHttpHandler();
        var httpClient = new HttpClient(handler);
        var client = new ZyfyClient(new ZyfyOptions
        {
            ApiKey = TestKey,
            HttpClient = httpClient,
        });
        return (client, handler);
    }

    [Fact]
    public async Task Lookup_BuildsCorrectUrl()
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk(VehicleJson());

        await client.Vehicle.LookupAsync("ab12 cde");

        var req = handler.Requests[0];
        Assert.Contains("/vehicle/AB12CDE", req.Url);
        Assert.Equal("GET", req.Method);
    }

    [Theory]
    [InlineData("ab12cde", "AB12CDE")]
    [InlineData("AB12 CDE", "AB12CDE")]
    [InlineData(" ab12cde ", "AB12CDE")]
    public async Task Lookup_NormalisesRegistration(string input, string expectedSuffix)
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk(VehicleJson());

        await client.Vehicle.LookupAsync(input);

        Assert.Contains($"/vehicle/{expectedSuffix}", handler.Requests[0].Url);
    }

    [Fact]
    public async Task BulkLookup_PostsToCorrectUrl()
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk(BulkVehicleJson());

        await client.Vehicle.BulkLookupAsync(new[] { "AB12CDE" });

        var req = handler.Requests[0];
        Assert.Contains("/vehicle/bulk", req.Url);
        Assert.Equal("POST", req.Method);

        using var doc = JsonDocument.Parse(req.Body);
        Assert.True(doc.RootElement.TryGetProperty("registrations", out _));
    }

    [Fact]
    public async Task SubmitBulk_PostsToAsyncUrl()
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk(JobJson("job-123"));

        await client.Vehicle.SubmitBulkAsync(new[] { "AB12CDE" });

        var req = handler.Requests[0];
        Assert.Contains("/vehicle/bulk/async", req.Url);
        Assert.Equal("POST", req.Method);
    }

    [Fact]
    public async Task GetJob_BuildsCorrectUrl()
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk(JobStatusJson("job-abc-123"));

        await client.Vehicle.GetJobAsync("job-abc-123");

        Assert.Contains("/vehicle/bulk/jobs/job-abc-123", handler.Requests[0].Url);
        Assert.Equal("GET", handler.Requests[0].Method);
    }

    [Fact]
    public async Task DeleteJob_BuildsCorrectUrl()
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk(@"{""deleted"":""job-abc-123""}");

        await client.Vehicle.DeleteJobAsync("job-abc-123");

        Assert.Contains("/vehicle/bulk/jobs/job-abc-123", handler.Requests[0].Url);
        Assert.Equal("DELETE", handler.Requests[0].Method);
    }
}
