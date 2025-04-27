using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.Serialization;

public class NullableStringConverter : JsonConverter<string?> {
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.TokenType switch {
            JsonTokenType.String when reader.GetString()!.Equals("null", StringComparison.CurrentCultureIgnoreCase) =>
                null,
            JsonTokenType.String when reader.GetString()!.Equals("nan", StringComparison.CurrentCultureIgnoreCase) =>
                null,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Null => null,
            _ => throw new JsonException("Invalid JSON value for Nullable<bool>.")
        };
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) {
        writer.WriteStringValue(value);
    }
}