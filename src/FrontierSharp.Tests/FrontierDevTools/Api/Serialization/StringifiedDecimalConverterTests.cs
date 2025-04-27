using System.Text.Json;
using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.Serialization;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api.Serialization;

public class StringifiedDecimalConverterTests {
    private readonly JsonSerializerOptions _options = new() {
        Converters = {
            new StringifiedDecimalConverter()
        }
    };

    [Theory]
    [InlineData("123.45", 123.45)]
    [InlineData("\"678.90\"", 678.90)]
    [InlineData("0", 0)]
    [InlineData("\"0.00\"", 0.00)]
    public void Read_ShouldParseValidDecimalValues(string jsonValue, decimal expected) {
        var json = $"{{\"Value\":{jsonValue}}}";

        var result = JsonSerializer.Deserialize<TestObject>(json, _options);

        result.Should().NotBeNull();
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("\"not-a-number\"", "not-a-number")]
    [InlineData("null", "")]
    public void Read_ShouldThrowJsonException_OnInvalidValue(string jsonValue, string expected) {
        var json = $"{{\"Value\":{jsonValue}}}";

        Action act = () => JsonSerializer.Deserialize<TestObject>(json, _options);

        act.Should().Throw<JsonException>()
            .WithMessage($"Invalid JSON value for Decimal ({expected}).");
    }

    [Theory]
    [InlineData(123.45, "\"123.45\"")]
    [InlineData(0, "\"0\"")]
    public void Write_ShouldSerializeDecimalAsString(decimal value, string expectedJsonValue) {
        var obj = new TestObject {
            Value = value
        };

        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Contain($"\"Value\":{expectedJsonValue}");
    }

    public class TestObject {
        public decimal Value { get; init; }
    }
}