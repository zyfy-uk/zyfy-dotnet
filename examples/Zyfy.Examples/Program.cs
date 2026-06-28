using System;
using System.Threading.Tasks;
using Zyfy.Examples;

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run -- <vehicle|postcode|nearest|within|bulk-vehicle|bulk-postcode|error <scenario>>");
    Console.WriteLine("Set ZYFY_API_KEY environment variable before running.");
    return 1;
}

switch (args[0])
{
    case "vehicle":
        await VehicleLookup.RunAsync();
        break;
    case "postcode":
        await PostcodeLookup.RunAsync();
        break;
    case "nearest":
        await NearestLookup.RunAsync();
        break;
    case "within":
        await WithinLookup.RunAsync();
        break;
    case "bulk-vehicle":
        await BulkVehicle.RunAsync();
        break;
    case "bulk-postcode":
        await BulkPostcode.RunAsync();
        break;
    case "error":
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: dotnet run -- error <scenario>");
            return 1;
        }
        await ErrorCases.RunAsync(args[1]);
        break;
    default:
        Console.WriteLine($"Unknown example: {args[0]}");
        Console.WriteLine("Valid options: vehicle, postcode, nearest, within, bulk-vehicle, bulk-postcode, error <scenario>");
        return 1;
}

return 0;
