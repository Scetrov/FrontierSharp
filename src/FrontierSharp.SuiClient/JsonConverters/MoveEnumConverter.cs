using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.SuiClient.JsonConverters;

public class MoveEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum {
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        switch (reader.TokenType) {
            case JsonTokenType.Number: {
                var intValue = reader.GetInt32();
                return (TEnum)Enum.ToObject(typeof(TEnum), intValue);
            }
            case JsonTokenType.String: {
                var strValue = reader.GetString()!;
                if (Enum.TryParse<TEnum>(strValue, true, out var parsed))
                    return parsed;
                throw new JsonException($"Unknown enum value '{strValue}' for {typeof(TEnum).Name}");
            }
            case JsonTokenType.StartObject: {
                using var doc = JsonDocument.ParseValue(ref reader);
                return ReadFromElement(doc.RootElement);
            }
            default:
                throw new JsonException($"Unexpected token type {reader.TokenType} for {typeof(TEnum).Name}");
        }
    }

    private static TEnum ReadFromElement(JsonElement element) {
        switch (element.ValueKind) {
            case JsonValueKind.Number:
                return (TEnum)Enum.ToObject(typeof(TEnum), element.GetInt32());
            case JsonValueKind.String: {
                var strValue = element.GetString()!;
                if (Enum.TryParse<TEnum>(strValue, true, out var parsed))
                    return parsed;
                throw new JsonException($"Unknown enum value '{strValue}' for {typeof(TEnum).Name}");
            }
            case JsonValueKind.Object: {
                if (element.TryGetProperty("@variant", out var variantElement)) {
                    if (variantElement.ValueKind != JsonValueKind.String)
                        throw new JsonException($"'@variant' must be a string for {typeof(TEnum).Name}");

                    var variantValue = variantElement.GetString()!;
                    if (Enum.TryParse<TEnum>(variantValue, true, out var variantResult))
                        return variantResult;

                    throw new JsonException($"Unknown enum variant '{variantValue}' for {typeof(TEnum).Name}");
                }

                using var enumerator = element.EnumerateObject();
                if (!enumerator.MoveNext())
                    throw new JsonException($"Empty object cannot be converted to {typeof(TEnum).Name}");

                var property = enumerator.Current;
                if (Enum.TryParse<TEnum>(property.Name, true, out var result))
                    return result;

                if (enumerator.MoveNext())
                    throw new JsonException($"Unknown enum variant '{property.Name}' for {typeof(TEnum).Name}");

                return ReadFromElement(property.Value);
            }
            default:
                throw new JsonException($"Unexpected JSON value kind {element.ValueKind} for {typeof(TEnum).Name}");
        }
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) {
        writer.WriteNumberValue(Convert.ToInt32(value));
    }
}