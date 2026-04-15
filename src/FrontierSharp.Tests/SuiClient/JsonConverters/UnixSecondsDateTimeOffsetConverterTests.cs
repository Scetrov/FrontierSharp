using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using FrontierSharp.SuiClient.JsonConverters;
using Xunit;

namespace FrontierSharp.Tests.SuiClient.JsonConverters;

public class UnixSecondsDateTimeOffsetConverterTests {
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public UnixSecondsDateTimeOffsetConverterTests() {
        _options.Converters.Add(new UnixSecondsDateTimeOffsetConverter());
    }

    [Fact]
    public void Read_UnixSecondsString_ParsesToDateTimeOffset() {
        var json = """{"value": "1700000000"}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1700000000));
    }

    [Fact]
    public void Read_UnixSecondsNumber_ParsesToDateTimeOffset() {
        var json = """{"value": 1700000000}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1700000000));
    }

    [Fact]
    public void Read_Iso8601String_ParsesToDateTimeOffset() {
        var json = """{"value": "2026-04-13T16:00:00+00:00"}""";
        var result = JsonSerializer.Deserialize<TestModel>(json, _options);
        result!.Value.Should().Be(DateTimeOffset.Parse("2026-04-13T16:00:00+00:00"));
    }

    [Fact]
    public void Read_InvalidToken_ThrowsJsonException() {
        var json = """{"value": true}""";
        var act = () => JsonSerializer.Deserialize<TestModel>(json, _options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Write_ProducesUnixSecondsString() {
        var model = new TestModel { Value = DateTimeOffset.FromUnixTimeSeconds(1700000000) };
        var json = JsonSerializer.Serialize(model, _options);
        json.Should().Contain("\"1700000000\"");
    }

    private class TestModel {
        [JsonConverter(typeof(UnixSecondsDateTimeOffsetConverter))]
        public DateTimeOffset Value { get; set; }
    }
}