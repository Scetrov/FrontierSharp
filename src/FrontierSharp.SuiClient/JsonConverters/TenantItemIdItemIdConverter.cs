using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.SuiClient.JsonConverters;

public class TenantItemIdItemIdConverter : JsonConverter<ulong> {
    public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.TokenType switch {
            JsonTokenType.String => ulong.Parse(reader.GetString()!),
            JsonTokenType.Number => reader.GetUInt64(),
            JsonTokenType.StartObject => ReadFromObject(ref reader),
            _ => throw new JsonException($"Cannot convert {reader.TokenType} to UInt64")
        };
    }

    public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }

    private static ulong ReadFromObject(ref Utf8JsonReader reader) {
        using var document = JsonDocument.ParseValue(ref reader);
        if (!document.RootElement.TryGetProperty("item_id", out var itemIdElement))
            throw new JsonException("TenantItemId object must contain an 'item_id' property.");

        return itemIdElement.ValueKind switch {
            JsonValueKind.String => ulong.Parse(itemIdElement.GetString()!),
            JsonValueKind.Number => itemIdElement.GetUInt64(),
            _ => throw new JsonException("TenantItemId 'item_id' must be a string or number.")
        };
    }
}

