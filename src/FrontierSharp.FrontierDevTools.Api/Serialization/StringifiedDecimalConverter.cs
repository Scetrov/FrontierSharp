using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.Serialization;

public class StringifiedDecimalConverter : JsonConverter<decimal> {
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var valueSpan = Encoding.UTF8.GetString(reader.ValueSpan);

        return reader.TokenType switch {
            JsonTokenType.Number when decimal.TryParse(valueSpan, out var decimalString) => decimalString,
            JsonTokenType.String when decimal.TryParse(reader.GetString(), out var decimalString) => decimalString,
            JsonTokenType.String when double.TryParse(reader.GetString(), out var doubleString) =>
                (decimal)doubleString,
            _ => throw new JsonException($"Invalid JSON value for Decimal ({reader.GetString()}).")
        };
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}