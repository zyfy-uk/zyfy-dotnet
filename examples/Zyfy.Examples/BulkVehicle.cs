using System;
using System.Threading.Tasks;
using Zyfy;
using Zyfy.Models;

namespace Zyfy.Examples;

internal static class BulkVehicle
{
    internal static async Task RunAsync()
    {
        using var client = new ZyfyClient();

        var registrations = new[] { "AB12CDE", "BD63SMR" };
        Console.WriteLine($"Bulk looking up {registrations.Length} vehicles...");

        var result = await client.Vehicle.BulkLookupAsync(registrations);
        Console.WriteLine($"Bulk total: {result.Total}");

        foreach (var item in result.Results)
        {
            if (item is VehicleResultItem success)
            {
                Console.WriteLine($"  {success.Result.Registration}: {success.Result.Make ?? "?"} {success.Result.Model ?? "?"} — {success.Result.VehicleType ?? "?"}");
            }
            else if (item is VehicleBulkItemError error)
            {
                Console.WriteLine($"  {error.Registration}: ERROR — {error.Error}");
            }
        }

        Console.WriteLine($"Quota remaining: {result.Quota.Remaining?.ToString() ?? "unlimited"}");
    }
}
