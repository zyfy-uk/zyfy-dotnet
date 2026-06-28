using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zyfy.Models;

/// <summary>
/// Current quota state, populated from response headers on every successful API call.
/// </summary>
public sealed record Quota
{
    /// <summary>
    /// Monthly request limit. <c>null</c> indicates an unlimited plan.
    /// </summary>
    [JsonConverter(typeof(QuotaValueConverter))]
    public long? Limit { get; init; }

    /// <summary>Requests used in the current billing period.</summary>
    public long Used { get; init; }

    /// <summary>
    /// Requests remaining in the current billing period. <c>null</c> indicates an unlimited plan.
    /// </summary>
    [JsonConverter(typeof(QuotaValueConverter))]
    public long? Remaining { get; init; }

    /// <summary>Grace limit (limit + 10%). <c>null</c> when not present in the response.</summary>
    public long? GraceLimit { get; init; }

    /// <summary>ISO 8601 datetime when the quota resets. <c>null</c> for unlimited plans.</summary>
    public string? Resets { get; init; }
}

internal sealed class QuotaValueConverter : JsonConverter<long?>
{
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (str == "unlimited")
            {
                return null;
            }
            if (long.TryParse(str, out var parsed))
            {
                return parsed;
            }
            return null;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt64();
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteStringValue("unlimited");
        }
        else
        {
            writer.WriteNumberValue(value.Value);
        }
    }
}
