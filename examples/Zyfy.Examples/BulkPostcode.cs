using System;
using System.Threading.Tasks;
using Zyfy;
using Zyfy.Models;

namespace Zyfy.Examples;

internal static class BulkPostcode
{
    internal static async Task RunAsync()
    {
        using var client = new ZyfyClient();

        var postcodes = new[] { "SW1A 2AA", "EC1A 1BB" };
        Console.WriteLine($"Bulk looking up {postcodes.Length} postcodes...");

        var result = await client.Postcode.BulkLookupAsync(postcodes);
        Console.WriteLine($"Bulk total: {result.Total}");

        foreach (var item in result.Results)
        {
            if (item is PostcodeResultItem success)
            {
                var crime = success.Result.Signals?.Crime?.RateBand ?? "n/a";
                var price = success.Result.Signals?.Property?.AveragePrice;
                var priceStr = price.HasValue ? $"£{price:N0}" : "n/a";
                Console.WriteLine($"  {success.Result.Postcode}: {success.Result.AdminDistrict ?? "?"} | crime: {crime} | avg price: {priceStr}");
            }
            else if (item is PostcodeBulkItemError error)
            {
                Console.WriteLine($"  {error.Postcode}: ERROR — {error.Error}");
            }
        }

        Console.WriteLine($"Quota remaining: {result.Quota.Remaining?.ToString() ?? "unlimited"}");
    }
}
