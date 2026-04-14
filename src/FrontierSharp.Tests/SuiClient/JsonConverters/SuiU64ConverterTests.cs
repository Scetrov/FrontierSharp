using System.Text.Json;
using AwesomeAssertions;
using FrontierSharp.SuiClient.JsonConverters;
using Xunit;

namespace FrontierSharp.Tests.SuiClient.JsonConverters;

public class SuiU64ConverterTests {
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public SuiU64ConverterTests() {
        _options.Converters.Add(new SuiU64Converter());
    }

    [Fact]
    public void Read_StringValue_ParsesCorrectly() {
        var json = """{"value": "18446744073709551615"}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().Be(ulong.MaxValue);
    }

    [Fact]
    public void Read_NumericValue_ParsesCorrectly() {
        var json = """{"value": 12345}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().Be(12345UL);
    }

    [Fact]
    public void Read_ZeroString_ParsesCorrectly() {
        var json = """{"value": "0"}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().Be(0UL);
    }

    [Fact]
    public void Read_InvalidToken_ThrowsJsonException() {
        var json = """{"value": true}""";
        var act = () => JsonSerializer.Deserialize<TestModel>(json, _options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Write_ProducesStringValue() {
        var model = new TestModel { Value = 42 };
        var json = JsonSerializer.Serialize(model, _options);
        json.Should().Contain("\"42\"");
    }

    private class TestModel {
        public ulong Value { get; set; }
    }
}