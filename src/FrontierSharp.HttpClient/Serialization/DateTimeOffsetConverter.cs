using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.HttpClient.Serialization;

public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset> {
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.String &&
            DateTimeOffset.TryParse(reader.GetString(), out var dateTimeOffset))
            return dateTimeOffset;

        if (reader.TokenType == JsonTokenType.String && long.TryParse(reader.GetString(), out var longValue))
            return DateTimeOffset.FromUnixTimeSeconds(longValue);

        if (reader.TokenType == JsonTokenType.Number)
            return DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64());

        throw new JsonException("Invalid JSON value for DateTimeOffset.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToUnixTimeMilliseconds().ToString());
    }
}