using System.Text.Json;
using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.Serialization;
using Xunit;

namespace FrontierSharp.FrontierDevTools.Tests.Api.Serialization;

public class NullableStringConverterTests {
    private readonly JsonSerializerOptions _options = new() {
        Converters = { new NullableStringConverter() }
    };

    [Theory]
    [InlineData("\"test\"", "test")]
    [InlineData("\"\"", "")]
    public void Read_ShouldReturnExpectedResult(string json, string expected) {
        // Act
        var result = JsonSerializer.Deserialize<string>(json, _options);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("\"null\"")]
    [InlineData("\"NULL\"")]
    [InlineData("\"Null\"")]
    [InlineData("\"nan\"")]
    [InlineData("\"NaN\"")]
    public void Read_ShouldReturnNull(string json) {
        // Act
        var result = JsonSerializer.Deserialize<string>(json, _options);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Read_ShouldThrowJsonException_WhenTokenIsNotStringOrNull() {
        // Arrange
        const string json = "123";

        // Act
        Action act = () => JsonSerializer.Deserialize<string>(json, _options);

        // Assert
        act.Should().Throw<JsonException>()
            .WithMessage("Invalid JSON value for Nullable<bool>.");
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("test", "\"test\"")]
    [InlineData("", "\"\"")]
    public void Write_ShouldSerializeAsExpected(string? value, string expectedJson) {
        // Act
        var json = JsonSerializer.Serialize(value, _options);

        // Assert
        json.Should().Be(expectedJson);
    }
}