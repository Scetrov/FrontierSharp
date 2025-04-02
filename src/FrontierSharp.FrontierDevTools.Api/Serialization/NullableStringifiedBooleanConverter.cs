using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.Serialization;

public class NullableStringifiedBooleanConverter : JsonConverter<bool?> {
    public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.TokenType switch {
            JsonTokenType.String when reader.GetString()!.Equals("null", StringComparison.CurrentCultureIgnoreCase) => null,
            JsonTokenType.String when reader.GetString()!.Equals("nan", StringComparison.CurrentCultureIgnoreCase) => null,
            JsonTokenType.String when bool.TryParse(reader.GetString(), out var boolString) => boolString,
            JsonTokenType.Null => null,
            _ => throw new JsonException("Invalid JSON value for Nullable<bool>.")
        };
    }

    public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options) {
        writer.WriteStringValue(value != null ? value.ToString() : "null");
    }
}