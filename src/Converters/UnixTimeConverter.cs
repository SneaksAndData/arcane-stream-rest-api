using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arcane.Stream.RestApi.Converters;

/// <summary>
/// Converts Unix time to/from DateTimeOffset for JSON serialization/deserialization
/// </summary>
public class UnixTimeConverter: JsonConverter<DateTimeOffset>
{
    
    /// <inheritdoc cref="JsonConverter{T}.Read"/>>
    public override DateTimeOffset Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64());

    /// <inheritdoc cref="JsonConverter{T}.Write"/>>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset dateTimeValue, JsonSerializerOptions options) =>
        writer.WriteNumberValue(dateTimeValue.ToUnixTimeMilliseconds());
}
