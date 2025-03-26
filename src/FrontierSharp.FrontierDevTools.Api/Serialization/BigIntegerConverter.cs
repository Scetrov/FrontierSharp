using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.Serialization;

public class BigIntegerConverter : JsonConverter<BigInteger> {
    public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return reader.TokenType switch {
            JsonTokenType.String when decimal.TryParse(reader.GetString(), out var decimalString) => new BigInteger(decimalString),
            JsonTokenType.String when BigInteger.TryParse(reader.GetString(), out var bigIntString) => bigIntString,
            JsonTokenType.Number when BigInteger.TryParse(Encoding.UTF8.GetString(reader.ValueSpan), out var bigIntNumeric) => bigIntNumeric,
            _ => throw new JsonException("Invalid JSON value for BigInteger.")
        };
    }

    public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }
}