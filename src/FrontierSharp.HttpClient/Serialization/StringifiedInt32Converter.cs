using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.HttpClient.Serialization;

public class StringifiedInt32Converter : JsonConverter<int> {
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var valueSpan = Encoding.UTF8.GetString(reader.ValueSpan);

        return reader.TokenType switch {
            JsonTokenType.Number when int.TryParse(valueSpan, out var intNumeric) => intNumeric,
            JsonTokenType.String when int.TryParse(reader.GetString(), out var intString) => intString,
            _ => throw new JsonException("Invalid JSON value for Int32.")
        };
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }
}