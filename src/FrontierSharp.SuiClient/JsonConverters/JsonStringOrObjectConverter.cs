using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.SuiClient.JsonConverters;

public class JsonStringOrObjectConverter : JsonConverter<string?> {
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.TokenType switch {
            JsonTokenType.Null => null,
            JsonTokenType.String => reader.GetString(),
            _ => JsonDocument.ParseValue(ref reader).RootElement.GetRawText()
        };
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) {
        if (value == null) {
            writer.WriteNullValue();
            return;
        }

        if (TryWriteStructuredJson(writer, value))
            return;

        writer.WriteStringValue(value);
    }

    private static bool TryWriteStructuredJson(Utf8JsonWriter writer, string value) {
        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return false;

        var looksLikeObject = trimmed.StartsWith("{") && trimmed.EndsWith("}");
        var looksLikeArray = trimmed.StartsWith("[") && trimmed.EndsWith("]");

        if (!looksLikeObject && !looksLikeArray)
            return false;

        try {
            using var document = JsonDocument.Parse(trimmed);
            document.RootElement.WriteTo(writer);
            return true;
        } catch (JsonException) {
            return false;
        }
    }
}
