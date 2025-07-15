using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

public class SmartAssemblyTypeConverter : JsonConverter<SmartAssemblyType> {
    public override SmartAssemblyType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        switch (reader.TokenType) {
            case JsonTokenType.String: {
                var enumText = reader.GetString();
                if (Enum.TryParse<SmartAssemblyType>(enumText, ignoreCase: true, out var value)) {
                    return value;
                }

                break;
            }
            case JsonTokenType.Number when reader.TryGetInt32(out int intValue) && Enum.IsDefined(typeof(SmartAssemblyType), intValue):
                return (SmartAssemblyType)intValue;
            default:
                return SmartAssemblyType.Unknown;
        }
    }

    public override void Write(Utf8JsonWriter writer, SmartAssemblyType value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }
}