using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.HttpClient.Serialization;

public class StringifiedBooleanConverter : JsonConverter<bool> {
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.TokenType switch {
            JsonTokenType.String when bool.TryParse(reader.GetString(), out var boolString) => boolString,
            _ => throw new JsonException("Invalid JSON value for Boolean.")
        };
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }
}