using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using FrontierSharp.SuiClient.JsonConverters;
using Xunit;

namespace FrontierSharp.Tests.SuiClient.JsonConverters;

public class LocationHashStringConverterTests {
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public LocationHashStringConverterTests() {
        _options.Converters.Add(new LocationHashStringConverter());
    }

    [Fact]
    public void Read_String_ParsesLocationHash() {
        var json = """{"value": "0xhashed_location"}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().Be("0xhashed_location");
    }

    [Fact]
    public void Read_Object_ParsesLocationHash() {
        var json = """{"value": {"location_hash": "wtTe4ujlqU+yJxH95e+wVb8tV+3EFtMlR4NHxWfX54c="}}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().Be("wtTe4ujlqU+yJxH95e+wVb8tV+3EFtMlR4NHxWfX54c=");
    }

    [Fact]
    public void Read_ObjectWithoutLocationHash_ThrowsJsonException() {
        var json = """{"value": {"unexpected": "value"}}""";
        var act = () => JsonSerializer.Deserialize<TestModel>(json, _options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Write_ProducesJsonString() {
        var model = new TestModel { Value = "0xhashed_location" };
        var json = JsonSerializer.Serialize(model, _options);
        json.Should().Contain("\"0xhashed_location\"");
    }

    private class TestModel {
        [JsonPropertyName("value")]
        [JsonConverter(typeof(LocationHashStringConverter))]
        public string Value { get; set; } = string.Empty;
    }
}

