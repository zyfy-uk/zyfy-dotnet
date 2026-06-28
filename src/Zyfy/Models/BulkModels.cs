using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zyfy.Models;

/// <summary>
/// A single item in a bulk vehicle response. Either a <see cref="VehicleResultItem"/>
/// or a <see cref="VehicleBulkItemError"/>.
/// </summary>
[JsonConverter(typeof(BulkVehicleItemConverter))]
public abstract record BulkVehicleItem;

/// <summary>A successful result within a bulk vehicle response.</summary>
public sealed record VehicleResultItem : BulkVehicleItem
{
    public VehicleResult Result { get; init; }

    public VehicleResultItem(VehicleResult result)
    {
        Result = result;
    }
}

/// <summary>An error item within a bulk vehicle response.</summary>
public sealed record VehicleBulkItemError : BulkVehicleItem
{
    public string Registration { get; init; } = string.Empty;
    /// <summary>not_found or invalid_format.</summary>
    public string Error { get; init; } = string.Empty;
}

/// <summary>
/// A single item in a bulk postcode response. Either a <see cref="PostcodeResultItem"/>
/// or a <see cref="PostcodeBulkItemError"/>.
/// </summary>
[JsonConverter(typeof(BulkPostcodeItemConverter))]
public abstract record BulkPostcodeItem;

/// <summary>A successful result within a bulk postcode response.</summary>
public sealed record PostcodeResultItem : BulkPostcodeItem
{
    public PostcodeResult Result { get; init; }

    public PostcodeResultItem(PostcodeResult result)
    {
        Result = result;
    }
}

/// <summary>An error item within a bulk postcode response.</summary>
public sealed record PostcodeBulkItemError : BulkPostcodeItem
{
    public string Postcode { get; init; } = string.Empty;
    /// <summary>not_found, unsupported_region, or invalid_format.</summary>
    public string Error { get; init; } = string.Empty;
}

/// <summary>Response from a synchronous bulk vehicle lookup.</summary>
public sealed record BulkVehicleResult
{
    public int Total { get; init; }
    /// <summary>Each item is either a <see cref="VehicleResultItem"/> or a <see cref="VehicleBulkItemError"/>.</summary>
    public BulkVehicleItem[] Results { get; init; } = Array.Empty<BulkVehicleItem>();
    public Quota Quota { get; init; } = new Quota();
}

/// <summary>Response from a synchronous bulk postcode lookup.</summary>
public sealed record BulkPostcodeResult
{
    public int Total { get; init; }
    /// <summary>Each item is either a <see cref="PostcodeResultItem"/> or a <see cref="PostcodeBulkItemError"/>.</summary>
    public BulkPostcodeItem[] Results { get; init; } = Array.Empty<BulkPostcodeItem>();
    public Quota Quota { get; init; } = new Quota();
}

/// <summary>Response when an async bulk job is submitted successfully.</summary>
public sealed record BulkJobSubmitted
{
    public string JobId { get; init; } = string.Empty;
    /// <summary>Always "queued" at submission time.</summary>
    public string Status { get; init; } = string.Empty;
    public int Total { get; init; }
    public string PollUrl { get; init; } = string.Empty;
    public Quota Quota { get; init; } = new Quota();
}

/// <summary>Status of an async bulk job.</summary>
/// <typeparam name="T">The result item type: <see cref="BulkVehicleItem"/> or <see cref="BulkPostcodeItem"/>.</typeparam>
public sealed record BulkJobStatus<T>
{
    public string JobId { get; init; } = string.Empty;
    /// <summary>queued, processing, complete, or expired.</summary>
    public string Status { get; init; } = string.Empty;
    public int Total { get; init; }
    public int Done { get; init; }
    /// <summary>ISO 8601 datetime when the job was created.</summary>
    public string CreatedAt { get; init; } = string.Empty;
    /// <summary>Null until status is "complete".</summary>
    public string? CompletedAt { get; init; }
    /// <summary>ISO 8601 datetime when the job results expire.</summary>
    public string ExpiresAt { get; init; } = string.Empty;
    /// <summary>Null until status is "complete".</summary>
    public T[]? Results { get; init; }
    public Quota Quota { get; init; } = new Quota();
}

/// <summary>Response returned when a bulk job is deleted.</summary>
public sealed record DeletedJob
{
    public string Deleted { get; init; } = string.Empty;
    public Quota Quota { get; init; } = new Quota();
}

internal sealed class BulkVehicleItemConverter : JsonConverter<BulkVehicleItem>
{
    public override BulkVehicleItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.TryGetProperty("error", out _))
        {
            var raw = root.GetRawText();
            return JsonSerializer.Deserialize<VehicleBulkItemError>(raw, options)!;
        }

        var resultRaw = root.GetRawText();
        var result = JsonSerializer.Deserialize<VehicleResult>(resultRaw, options)!;
        return new VehicleResultItem(result);
    }

    public override void Write(Utf8JsonWriter writer, BulkVehicleItem value, JsonSerializerOptions options)
    {
        if (value is VehicleResultItem item)
        {
            JsonSerializer.Serialize(writer, item.Result, options);
        }
        else if (value is VehicleBulkItemError error)
        {
            JsonSerializer.Serialize(writer, error, options);
        }
    }
}

internal sealed class BulkPostcodeItemConverter : JsonConverter<BulkPostcodeItem>
{
    public override BulkPostcodeItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.TryGetProperty("error", out _))
        {
            var raw = root.GetRawText();
            return JsonSerializer.Deserialize<PostcodeBulkItemError>(raw, options)!;
        }

        var resultRaw = root.GetRawText();
        var result = JsonSerializer.Deserialize<PostcodeResult>(resultRaw, options)!;
        return new PostcodeResultItem(result);
    }

    public override void Write(Utf8JsonWriter writer, BulkPostcodeItem value, JsonSerializerOptions options)
    {
        if (value is PostcodeResultItem item)
        {
            JsonSerializer.Serialize(writer, item.Result, options);
        }
        else if (value is PostcodeBulkItemError error)
        {
            JsonSerializer.Serialize(writer, error, options);
        }
    }
}
