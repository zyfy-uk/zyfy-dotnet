using System;
using System.Globalization;
using System.Threading.Tasks;
using Zyfy;

namespace Zyfy.Examples;

internal static class NearestLookup
{
    internal static async Task RunAsync()
    {
        var lat = double.Parse(Environment.GetEnvironmentVariable("LAT") ?? "51.508", CultureInfo.InvariantCulture);
        var lon = double.Parse(Environment.GetEnvironmentVariable("LON") ?? "-0.1281", CultureInfo.InvariantCulture);

        using var client = new ZyfyClient();
        var result = await client.Postcode.NearestAsync(lat, lon);

        Console.WriteLine($"Nearest postcode: {result.Postcode}");
        Console.WriteLine($"Distance (m):     {result.QueryPointDistanceMetres}");
        Console.WriteLine($"Country / Region: {result.Country} / {result.Region}");
        Console.WriteLine($"Admin district:   {result.AdminDistrict ?? "n/a"}");
        Console.WriteLine($"Flood risk:       {result.Signals?.Flood?.RiversSea ?? "n/a"}");
        Console.WriteLine($"Crime rate band:  {result.Signals?.Crime?.RateBand ?? "n/a"}");
        Console.WriteLine($"Liveability:      {result.Summary?.LiveabilityLevel ?? "n/a"}");
        Console.WriteLine($"Quota remaining:  {result.Quota?.Remaining?.ToString() ?? "unlimited"}");
    }
}
