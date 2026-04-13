using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using FrontierSharp.SuiClient.JsonConverters;
using Xunit;

namespace FrontierSharp.Tests.SuiClient.JsonConverters;

public class JsonStringOrObjectConverterTests {
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public JsonStringOrObjectConverterTests() {
        _options.Converters.Add(new JsonStringOrObjectConverter());
    }

    [Fact]
    public void Read_String_ParsesToString() {
        var json = """{"value": "plain-text"}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().Be("plain-text");
    }

    [Fact]
    public void Read_Object_ParsesToRawJsonString() {
        var json = """{"value": {"name": "EventHorizon", "url": ""}}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);

        using var metadataDocument = JsonDocument.Parse(result!.Value!);
        metadataDocument.RootElement.GetProperty("name").GetString().Should().Be("EventHorizon");
        metadataDocument.RootElement.GetProperty("url").GetString().Should().BeEmpty();
    }

    [Fact]
    public void Read_Null_ParsesToNull() {
        var json = """{"value": null}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().BeNull();
    }

    [Fact]
    public void Write_PlainString_ProducesJsonString() {
        var model = new TestModel { Value = "plain-text" };
        var json = JsonSerializer.Serialize(model, _options);
        json.Should().Contain("\"plain-text\"");
    }

    [Fact]
    public void Write_RawJsonObjectString_ProducesJsonObject() {
        var model = new TestModel { Value = "{\"name\":\"EventHorizon\",\"url\":\"\"}" };
        var json = JsonSerializer.Serialize(model, _options);

        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("value").GetProperty("name").GetString().Should().Be("EventHorizon");
        document.RootElement.GetProperty("value").GetProperty("url").GetString().Should().BeEmpty();
    }

    private class TestModel {
        [JsonPropertyName("value")]
        [JsonConverter(typeof(JsonStringOrObjectConverter))]
        public string? Value { get; set; }
    }
}


