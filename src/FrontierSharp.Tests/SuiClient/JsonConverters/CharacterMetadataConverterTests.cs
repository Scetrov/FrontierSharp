using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using FrontierSharp.SuiClient.JsonConverters;
using FrontierSharp.SuiClient.Models;
using Xunit;

namespace FrontierSharp.Tests.SuiClient.JsonConverters;

public class CharacterMetadataConverterTests {
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public CharacterMetadataConverterTests() {
        _options.Converters.Add(new CharacterMetadataConverter());
    }

    [Fact]
    public void Read_String_PreservesLegacyRawValue() {
        var json = """{"value": "plain-text"}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().NotBeNull();
        result.Value!.RawValue.Should().Be("plain-text");
    }

    [Fact]
    public void Read_Object_ParsesToCharacterMetadata() {
        var json = """{"value": {"assembly_id": "0xassembly", "name": "EventHorizon", "url": ""}}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().NotBeNull();
        result.Value!.AssemblyId.Should().Be("0xassembly");
        result.Value.Name.Should().Be("EventHorizon");
        result.Value.Url.Should().BeEmpty();
    }

    [Fact]
    public void Read_Null_ParsesToNull() {
        var json = """{"value": null}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().BeNull();
    }

    [Fact]
    public void Read_UnknownProperty_CapturesExtensionData() {
        var json = """{"value": {"name": "EventHorizon", "rarity": "legendary"}}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);

        result!.Value.Should().NotBeNull();
        result.Value!.AdditionalProperties.Should().NotBeNull();
        result.Value.AdditionalProperties!["rarity"].GetString().Should().Be("legendary");
    }

    [Fact]
    public void Write_LegacyRawValue_ProducesJsonString() {
        var model = new TestModel { Value = new CharacterMetadata { RawValue = "plain-text" } };
        var json = JsonSerializer.Serialize(model, _options);
        json.Should().Contain("\"plain-text\"");
    }

    [Fact]
    public void Write_MetadataObject_ProducesJsonObject() {
        var model = new TestModel {
            Value = new CharacterMetadata {
                AssemblyId = "0xassembly",
                Name = "EventHorizon",
                Url = ""
            }
        };
        var json = JsonSerializer.Serialize(model, _options);

        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("value").GetProperty("assembly_id").GetString().Should().Be("0xassembly");
        document.RootElement.GetProperty("value").GetProperty("name").GetString().Should().Be("EventHorizon");
        document.RootElement.GetProperty("value").GetProperty("url").GetString().Should().BeEmpty();
    }

    private class TestModel {
        [JsonPropertyName("value")]
        [JsonConverter(typeof(CharacterMetadataConverter))]
        public CharacterMetadata? Value { get; set; }
    }
}



