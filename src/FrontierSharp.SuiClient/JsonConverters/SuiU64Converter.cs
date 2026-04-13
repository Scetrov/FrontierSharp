using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.SuiClient.JsonConverters;

public class SuiU64Converter : JsonConverter<ulong> {
    public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.TokenType switch {
            JsonTokenType.String => ulong.Parse(reader.GetString()!),
            JsonTokenType.Number => reader.GetUInt64(),
            _ => throw new JsonException($"Cannot convert {reader.TokenType} to UInt64")
        };
    }

    public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }
}

