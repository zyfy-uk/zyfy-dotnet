using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Zyfy;

namespace Zyfy.Examples;

internal static class WithinLookup
{
    internal static async Task RunAsync()
    {
        var lat    = double.Parse(Environment.GetEnvironmentVariable("LAT")    ?? "51.508",  CultureInfo.InvariantCulture);
        var lon    = double.Parse(Environment.GetEnvironmentVariable("LON")    ?? "-0.1281", CultureInfo.InvariantCulture);
        var radius = int.Parse(Environment.GetEnvironmentVariable("RADIUS") ?? "500");

        using var client = new ZyfyClient();
        var result = await client.Postcode.WithinAsync(lat, lon, radius);

        Console.WriteLine($"Found: {result.Total}");

        var closest = result.Results.FirstOrDefault();
        if (closest is not null)
        {
            Console.WriteLine($"Closest postcode: {closest.Postcode}");
        }

        foreach (var pc in result.Results)
        {
            var flood = pc.Signals?.Flood?.RiversSea ?? "—";
            var crime = pc.Signals?.Crime?.RateBand ?? "—";
            Console.WriteLine($"  {pc.Postcode,-9} {pc.QueryPointDistanceMetres,8:F1} m  flood={flood}  crime={crime}");
        }

        Console.WriteLine($"Quota remaining: {result.Quota?.Remaining?.ToString() ?? "unlimited"}");
    }
}
