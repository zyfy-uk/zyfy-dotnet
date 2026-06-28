using System;
using Zyfy.Internal;
using Zyfy.Resources;

namespace Zyfy;

/// <summary>
/// Official .NET client for the Zyfy UK data enrichment API.
/// Provides <see cref="Vehicle"/> and <see cref="Postcode"/> resource namespaces,
/// each with both synchronous and asynchronous methods.
/// </summary>
/// <example>
/// <code>
/// var client = new ZyfyClient("ea_live_...");
///
/// // Async
/// var vehicle = await client.Vehicle.LookupAsync("AB12CDE");
/// Console.WriteLine(vehicle.Make);
///
/// // Sync
/// var postcode = client.Postcode.Lookup("SW1A 2AA");
/// Console.WriteLine(postcode.AdminDistrict);
/// </code>
/// </example>
public sealed class ZyfyClient : IDisposable
{
    private readonly HttpTransport _transport;

    public VehicleResource Vehicle { get; }
    public PostcodeResource Postcode { get; }

    /// <summary>
    /// Initialise a new <see cref="ZyfyClient"/> with the given API key.
    /// </summary>
    /// <param name="apiKey">Your Zyfy API key.</param>
    /// <exception cref="InvalidOperationException">Thrown when no API key is provided and <c>ZYFY_API_KEY</c> is not set.</exception>
    public ZyfyClient(string apiKey)
        : this(new ZyfyOptions { ApiKey = apiKey })
    {
    }

    /// <summary>
    /// Initialise a new <see cref="ZyfyClient"/> with the given options.
    /// Falls back to the <c>ZYFY_API_KEY</c> environment variable when <see cref="ZyfyOptions.ApiKey"/> is not set.
    /// </summary>
    /// <param name="options">Client configuration options.</param>
    /// <exception cref="InvalidOperationException">Thrown when no API key is provided and <c>ZYFY_API_KEY</c> is not set.</exception>
    public ZyfyClient(ZyfyOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _transport = new HttpTransport(options);
        Vehicle = new VehicleResource(_transport);
        Postcode = new PostcodeResource(_transport);
    }

    /// <summary>
    /// Initialise a new <see cref="ZyfyClient"/> using defaults.
    /// The API key must be set in the <c>ZYFY_API_KEY</c> environment variable.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <c>ZYFY_API_KEY</c> is not set.</exception>
    public ZyfyClient()
        : this(new ZyfyOptions())
    {
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _transport.Dispose();
    }
}
