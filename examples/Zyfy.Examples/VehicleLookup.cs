using System;
using System.Threading.Tasks;
using Zyfy;

namespace Zyfy.Examples;

internal static class VehicleLookup
{
    internal static async Task RunAsync()
    {
        var registration = Environment.GetEnvironmentVariable("VEHICLE_REG");
        if (string.IsNullOrEmpty(registration))
        {
            Console.Error.WriteLine("Set VEHICLE_REG to a UK registration mark, e.g. VEHICLE_REG=AB12CDE");
            Environment.Exit(1);
        }

        using var client = new ZyfyClient();
        var result = await client.Vehicle.LookupAsync(registration);

        Console.WriteLine($"Registration: {result.Registration}");
        Console.WriteLine($"Make / Model: {result.Make} {result.Model}");
        Console.WriteLine($"Vehicle type: {result.VehicleType ?? "n/a"}");
        Console.WriteLine($"Year: {result.YearOfManufacture?.ToString() ?? "n/a"}");
        Console.WriteLine($"Fuel type: {result.FuelType ?? "n/a"}");
        Console.WriteLine($"ULEZ compliant: {result.Signals?.UlezCompliant?.ToString() ?? "n/a"}");
        Console.WriteLine($"MOT status: {result.Signals?.MotStatus ?? "n/a"}");
        Console.WriteLine($"MOT expiry: {result.Signals?.MotExpiryDate ?? "n/a"}");
        Console.WriteLine($"MOT pass rate: {result.Signals?.MotPassRate?.ToString() ?? "n/a"}");
        Console.WriteLine($"Buy recommendation: {result.Summary?.BuyRecommendation ?? "n/a"}");
        Console.WriteLine($"Enrichment pending: {result.EnrichmentPending}");
        Console.WriteLine($"Quota remaining: {result.Quota?.Remaining?.ToString() ?? "n/a"}");
    }
}
