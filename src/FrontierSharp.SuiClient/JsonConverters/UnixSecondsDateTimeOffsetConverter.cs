using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.SuiClient.JsonConverters;

public class UnixSecondsDateTimeOffsetConverter : JsonConverter<DateTimeOffset> {
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.String) {
            var stringValue = reader.GetString()!;
            if (long.TryParse(stringValue, out var unixSeconds))
                return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

            if (DateTimeOffset.TryParse(stringValue, out var parsedDateTimeOffset))
                return parsedDateTimeOffset;
        }

        if (reader.TokenType == JsonTokenType.Number)
            return DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64());

        throw new JsonException("Invalid JSON value for DateTimeOffset.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToUnixTimeSeconds().ToString());
    }
}

