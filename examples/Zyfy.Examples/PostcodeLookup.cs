using System;
using System.Threading.Tasks;
using Zyfy;

namespace Zyfy.Examples;

internal static class PostcodeLookup
{
    internal static async Task RunAsync()
    {
        var postcode = Environment.GetEnvironmentVariable("POSTCODE") ?? "SW1A 2AA";

        using var client = new ZyfyClient();
        var result = await client.Postcode.LookupAsync(postcode);

        var signals = result.Signals;
        Console.WriteLine($"Postcode: {result.Postcode}");
        Console.WriteLine($"Country / Region: {result.Country} / {result.Region}");
        Console.WriteLine($"Admin district: {result.AdminDistrict ?? "n/a"}");
        Console.WriteLine($"MP: {signals?.Political?.MpName ?? "n/a"} ({signals?.Political?.MpParty ?? "n/a"})");
        Console.WriteLine($"Flood risk (rivers/sea): {signals?.Flood?.RiversSea ?? "n/a"}");
        Console.WriteLine($"Crime rate band: {signals?.Crime?.RateBand ?? "n/a"}");
        Console.WriteLine($"Average property price: {signals?.Property?.AveragePrice?.ToString() ?? "n/a"}");
        Console.WriteLine($"EPC average rating: {signals?.Housing?.EpcAverageRating ?? "n/a"}");
        Console.WriteLine($"Broadband gigabit: {signals?.Broadband?.Gigabit?.ToString() ?? "n/a"}");
        Console.WriteLine($"IMD decile (1=most deprived): {signals?.Deprivation?.ImdDecile?.ToString() ?? "n/a"}");
        Console.WriteLine($"Liveability level: {result.Summary?.LiveabilityLevel ?? "n/a"}");
        Console.WriteLine($"Investment outlook: {result.Summary?.InvestmentOutlook ?? "n/a"}");
        Console.WriteLine($"Quota remaining: {result.Quota?.Remaining?.ToString() ?? "n/a"}");
    }
}
