using FrontierSharp.FrontierDevTools.Api.Serialization;

namespace FrontierSharp.FrontierDevTools.Tests.Api.Serialization;

using System;
using System.Text.Json;
using FluentAssertions;
using Xunit;

public class NullableStringifiedBooleanConverterTests
{
    private readonly JsonSerializerOptions _options = new() {
        Converters = { new NullableStringifiedBooleanConverter() }
    };

    [Theory]
    [InlineData("\"null\"", null)]
    [InlineData("\"NULL\"", null)]
    [InlineData("\"nUlL\"", null)]
    [InlineData("\"nan\"", null)]
    [InlineData("\"NaN\"", null)]
    [InlineData("\"true\"", true)]
    [InlineData("\"false\"", false)]
    public void Read_ShouldReturnExpectedResult(string json, bool? expected)
    {
        // Act
        var result = JsonSerializer.Deserialize<bool?>(json, _options);
        
        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Read_ShouldReturnNull_WhenTokenIsNull()
    {
        // Arrange
        const string json = "null";
        
        // Act
        var result = JsonSerializer.Deserialize<bool?>(json, _options);
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Read_ShouldThrowJsonException_WhenTokenIsInvalid()
    {
        // Arrange
        const string json = "123"; // Not a string and not null
        
        // Act
        Action act = () => JsonSerializer.Deserialize<bool?>(json, _options);
        
        // Assert
        act.Should().Throw<JsonException>()
            .WithMessage("Invalid JSON value for Nullable<bool>.");
    }

    [Theory]
    [InlineData(true, "\"True\"")]
    [InlineData(false, "\"False\"")]
    [InlineData(null, "null")]
    public void Write_ShouldSerializeAsExpected(bool? value, string expectedJson)
    {
        // Act
        var json = JsonSerializer.Serialize(value, _options);
        
        // Assert
        json.Should().Be(expectedJson);
    }
}
