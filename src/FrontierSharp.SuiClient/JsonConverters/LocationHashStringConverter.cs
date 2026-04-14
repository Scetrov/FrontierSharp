using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.SuiClient.JsonConverters;

public class LocationHashStringConverter : JsonConverter<string> {
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.TokenType switch {
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Null => string.Empty,
            JsonTokenType.StartObject => ReadFromObject(ref reader),
            _ => throw new JsonException("Invalid JSON value for Assembly location.")
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) {
        writer.WriteStringValue(value);
    }

    private static string ReadFromObject(ref Utf8JsonReader reader) {
        using var document = JsonDocument.ParseValue(ref reader);
        if (!document.RootElement.TryGetProperty("location_hash", out var locationHashElement))
            throw new JsonException("Assembly location object must contain a 'location_hash' property.");

        return locationHashElement.ValueKind switch {
            JsonValueKind.String => locationHashElement.GetString() ?? string.Empty,
            JsonValueKind.Null => string.Empty,
            _ => throw new JsonException("Assembly location 'location_hash' must be a string or null.")
        };
    }
}