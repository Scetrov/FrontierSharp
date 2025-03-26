using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.Serialization;

public class StringifiedBooleanConverter : JsonConverter<bool> {
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var valueSpan = Encoding.UTF8.GetString(reader.ValueSpan);

        return reader.TokenType switch {
            JsonTokenType.Number when bool.TryParse(valueSpan, out var intNumeric) => intNumeric,
            JsonTokenType.String when bool.TryParse(reader.GetString(), out var intString) => intString,
            _ => throw new JsonException("Invalid JSON value for Int32.")
        };
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }
}