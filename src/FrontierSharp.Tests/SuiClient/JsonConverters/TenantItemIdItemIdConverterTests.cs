using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using FrontierSharp.SuiClient.JsonConverters;
using Xunit;

namespace FrontierSharp.Tests.SuiClient.JsonConverters;

public class TenantItemIdItemIdConverterTests {
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public TenantItemIdItemIdConverterTests() {
        _options.Converters.Add(new TenantItemIdItemIdConverter());
    }

    [Fact]
    public void Read_ObjectValue_ParsesItemIdAndIgnoresTenant() {
        var json = """{"value": {"item_id": "12345", "tenant": "stillness"}}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().Be(12345UL);
    }

    [Fact]
    public void Read_StringValue_ParsesCorrectly() {
        var json = """{"value": "42"}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().Be(42UL);
    }

    [Fact]
    public void Read_NumberValue_ParsesCorrectly() {
        var json = """{"value": 42}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().Be(42UL);
    }

    [Fact]
    public void Read_ObjectWithoutItemId_ThrowsJsonException() {
        var json = """{"value": {"tenant": "stillness"}}""";
        var act = () => JsonSerializer.Deserialize<TestModel>(json, _options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Write_ProducesStringValue() {
        var model = new TestModel { Value = 42UL };
        var json = JsonSerializer.Serialize(model, _options);
        json.Should().Contain("\"42\"");
    }

    private class TestModel {
        [JsonConverter(typeof(TenantItemIdItemIdConverter))]
        public ulong Value { get; set; }
    }
}