using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Zyfy.Tests.Helpers;

namespace Zyfy.Tests;

public sealed class PostcodeResourceTests
{
    private const string TestKey = "ea_live_test";

    private static string PostcodeJson() =>
        @"{""postcode"":""SW1A 2AA"",""outwardCode"":""SW1A"",""inwardCode"":""2AA"",
""latitude"":51.5,"" longitude"":-0.1,""eastings"":null,""northings"":null,
""country"":""England"",""region"":""London"",""adminDistrict"":""City of Westminster"",
""adminCounty"":null,""adminWard"":null,""parish"":null,""parliamentaryConstituency"":null,
""nhsTrust"":null,""lsoa"":null,""msoa"":null,""ruralUrbanClassification"":null,
""summary"":null,""signals"":null,""scores"":null,""percentiles"":null,
""geographyCodes"":{""adminDistrict"":null,""adminCounty"":null,""adminWard"":null,""parliamentaryConstituency"":null,""lsoa"":null,""msoa"":null},
""queryPointDistanceMetres"":null,
""sources"":{""geography"":""ONS"",""flood"":""EA"",""crime"":""police.uk"",""property"":""HMLR"",""deprivation"":""MHCLG"",""broadband"":""Ofcom"",""environment"":""DEFRA"",""epc"":""DLUHC"",""greenSpace"":""OS"",""demographics"":null},
""schemaVersion"":""1"",""dataAsOf"":""2026-01-01T00:00:00Z"",""checkedAt"":""2026-01-01T00:00:00Z""}";

    private static string WithinJson() =>
        $@"{{""total"":1,""results"":[{PostcodeJson()}]}}";

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
        handler.EnqueueOk(PostcodeJson());

        await client.Postcode.LookupAsync("SW1A 2AA");

        Assert.Contains("/postcode/SW1A%202AA", handler.Requests[0].Url);
        Assert.Equal("GET", handler.Requests[0].Method);
    }

    [Theory]
    [InlineData("sw1a 2aa", "SW1A%202AA")]
    [InlineData("  SW1A 2AA  ", "SW1A%202AA")]
    public async Task Lookup_NormalisesPostcode(string input, string expectedEncoded)
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk(PostcodeJson());

        await client.Postcode.LookupAsync(input);

        Assert.Contains($"/postcode/{expectedEncoded}", handler.Requests[0].Url);
    }

    [Fact]
    public async Task Nearest_BuildsCorrectQueryString()
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk(PostcodeJson());

        await client.Postcode.NearestAsync(51.5, -0.1);

        var url = handler.Requests[0].Url;
        Assert.Contains("/postcode/nearest", url);
        Assert.Contains("lat=51.5", url);
        Assert.Contains("lon=-0.1", url);
    }

    [Fact]
    public async Task Nearest_WithRadius_IncludesRadiusParam()
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk(PostcodeJson());

        await client.Postcode.NearestAsync(51.5, -0.1, radius: 500);

        Assert.Contains("radius=500", handler.Requests[0].Url);
    }

    [Fact]
    public async Task Within_BuildsCorrectQueryString()
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk(WithinJson());

        await client.Postcode.WithinAsync(51.5, -0.1);

        var url = handler.Requests[0].Url;
        Assert.Contains("/postcode/within", url);
        Assert.Contains("lat=51.5", url);
        Assert.Contains("lon=-0.1", url);
    }

    [Fact]
    public async Task Within_WithRadius_IncludesRadiusParam()
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk(WithinJson());

        await client.Postcode.WithinAsync(51.5, -0.1, radius: 2000);

        Assert.Contains("radius=2000", handler.Requests[0].Url);
    }

    [Fact]
    public async Task Within_WithoutRadius_NoRadiusParam()
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk(WithinJson());

        await client.Postcode.WithinAsync(51.5, -0.1);

        Assert.DoesNotContain("radius=", handler.Requests[0].Url);
    }

    [Fact]
    public async Task BulkLookup_PostsToCorrectUrl()
    {
        var (client, handler) = MakeClient();
        handler.EnqueueOk($@"{{""total"":1,""results"":[{PostcodeJson()}]}}");

        await client.Postcode.BulkLookupAsync(new[] { "SW1A 2AA" });

        var req = handler.Requests[0];
        Assert.Contains("/postcode/bulk", req.Url);
        Assert.Equal("POST", req.Method);
        using var doc = JsonDocument.Parse(req.Body);
        Assert.True(doc.RootElement.TryGetProperty("postcodes", out _));
    }
}
