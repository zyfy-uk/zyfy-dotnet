using System;
using System.Threading.Tasks;
using Zyfy;

namespace Zyfy.Examples;

/// <summary>
/// Error scenario examples used by compare-sdks.sh to verify the SDK raises
/// the correct exception type for known error conditions.
///
/// Usage: dotnet run -- error &lt;scenario&gt;
///
/// Scenarios:
///   vehicle-invalid       Malformed registration - ApiException (400)
///   postcode-not-found    Valid format but nonexistent postcode - NotFoundException (404)
///   postcode-ni           Northern Ireland BT postcode - ValidationException (422)
///   bad-auth              Invalid API key - AuthenticationException (401)
///   nearest-out-of-uk     Coordinates outside UK bounding box - ApiException (400)
///   within-no-results     Tiny radius with no postcodes - NotFoundException (404)
/// </summary>
internal static class ErrorCases
{
    internal static async Task RunAsync(string scenario)
    {
        try
        {
            switch (scenario)
            {
                case "vehicle-invalid":
                    using (var c = new ZyfyClient())
                        await c.Vehicle.LookupAsync("!!INVALID!!");
                    break;

                case "postcode-not-found":
                    using (var c = new ZyfyClient())
                        await c.Postcode.LookupAsync("ZZ1 1ZZ");
                    break;

                case "postcode-ni":
                    using (var c = new ZyfyClient())
                        await c.Postcode.LookupAsync("BT1 1AA");
                    break;

                case "bad-auth":
                    using (var c = new ZyfyClient(new ZyfyOptions { ApiKey = "ea_live_thisisnotavalidkey" }))
                        await c.Vehicle.LookupAsync("AB12CDE");
                    break;

                case "nearest-out-of-uk":
                    using (var c = new ZyfyClient())
                        await c.Postcode.NearestAsync(48.8566, 2.3522);
                    break;

                case "within-no-results":
                    using (var c = new ZyfyClient())
                        await c.Postcode.WithinAsync(54.0, 2.0, 1);
                    break;

                default:
                    Console.Error.WriteLine($"Unknown error scenario: {scenario}");
                    Environment.Exit(1);
                    return;
            }

            Console.WriteLine("Error type: none");
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"Error type: {ex.GetType().Name}");
            Console.WriteLine($"Error code: {ex.Code}");
        }
        catch (ZyfyException ex)
        {
            Console.WriteLine($"Error type: {ex.GetType().Name}");
        }
    }
}
