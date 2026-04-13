using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.SuiClient.JsonConverters;

public class CharacterMetadataConverter : JsonConverter<FrontierSharp.SuiClient.Models.CharacterMetadata?> {
    public override FrontierSharp.SuiClient.Models.CharacterMetadata? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.TokenType switch {
            JsonTokenType.Null => null,
            JsonTokenType.String => new FrontierSharp.SuiClient.Models.CharacterMetadata { RawValue = reader.GetString() },
            JsonTokenType.StartObject => ReadObject(ref reader),
            _ => throw new JsonException("Invalid JSON value for CharacterMetadata.")
        };
    }

    public override void Write(Utf8JsonWriter writer, FrontierSharp.SuiClient.Models.CharacterMetadata? value, JsonSerializerOptions options) {
        if (value == null) {
            writer.WriteNullValue();
            return;
        }

        if (ShouldWriteRawValue(value)) {
            writer.WriteStringValue(value.RawValue);
            return;
        }

        writer.WriteStartObject();

        WriteStringProperty(writer, "assembly_id", value.AssemblyId);
        WriteStringProperty(writer, "name", value.Name);
        WriteStringProperty(writer, "description", value.Description);
        WriteStringProperty(writer, "url", value.Url);

        if (value.AdditionalProperties != null) {
            foreach (var property in value.AdditionalProperties) {
                writer.WritePropertyName(property.Key);
                property.Value.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }

    private static FrontierSharp.SuiClient.Models.CharacterMetadata ReadObject(ref Utf8JsonReader reader) {
        using var document = JsonDocument.ParseValue(ref reader);
        var metadata = new FrontierSharp.SuiClient.Models.CharacterMetadata();
        Dictionary<string, JsonElement>? additionalProperties = null;

        foreach (var property in document.RootElement.EnumerateObject()) {
            switch (property.Name) {
                case "assembly_id":
                    metadata.AssemblyId = ReadStringValue(property.Value, property.Name);
                    break;
                case "name":
                    metadata.Name = ReadStringValue(property.Value, property.Name);
                    break;
                case "description":
                    metadata.Description = ReadStringValue(property.Value, property.Name);
                    break;
                case "url":
                    metadata.Url = ReadStringValue(property.Value, property.Name);
                    break;
                default:
                    additionalProperties ??= [];
                    additionalProperties[property.Name] = property.Value.Clone();
                    break;
            }
        }

        metadata.AdditionalProperties = additionalProperties;
        return metadata;
    }

    private static string? ReadStringValue(JsonElement element, string propertyName) {
        return element.ValueKind switch {
            JsonValueKind.Null => null,
            JsonValueKind.String => element.GetString(),
            _ => throw new JsonException($"Character metadata property '{propertyName}' must be a string or null.")
        };
    }

    private static bool ShouldWriteRawValue(FrontierSharp.SuiClient.Models.CharacterMetadata value) {
        return value.RawValue != null &&
               value.AssemblyId == null &&
               value.Name == null &&
               value.Description == null &&
               value.Url == null &&
               (value.AdditionalProperties == null || value.AdditionalProperties.Count == 0);
    }

    private static void WriteStringProperty(Utf8JsonWriter writer, string propertyName, string? value) {
        if (value != null)
            writer.WriteString(propertyName, value);
    }
}


