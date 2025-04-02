using System.Text;
using System.Text.Json;
using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.Serialization;
using Xunit;

namespace FrontierSharp.FrontierDevTools.Tests.Api.Serialization;

public class StringifiedBooleanConverterTests {
    [Fact]
    public void Read_WithValidStringTrue_ShouldReturnTrue() {
        // Arrange: A JSON string representing "true".
        var converter = new StringifiedBooleanConverter();
        const string json = "\"true\"";
        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read(); // Advance to the token.

        // Act
        var result = converter.Read(ref reader, typeof(bool), new JsonSerializerOptions());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Read_WithValidStringFalse_ShouldReturnFalse() {
        // Arrange: A JSON string representing "false".
        var converter = new StringifiedBooleanConverter();
        const string json = "\"false\"";
        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read(); // Advance to the token.

        // Act
        var result = converter.Read(ref reader, typeof(bool), new JsonSerializerOptions());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Read_WithNumberTokenOne_ShouldThrowJsonException() {
        // Arrange: A JSON number token "1".
        var converter = new StringifiedBooleanConverter();
        const string json = "1";
        var bytes = Encoding.UTF8.GetBytes(json);

        // Act: Create the reader inside the lambda to avoid capturing a ref local.
        var act = () => {
            var reader = new Utf8JsonReader(bytes);
            reader.Read(); // Advance to the number token.
            converter.Read(ref reader, typeof(bool), new JsonSerializerOptions());
        };

        // Assert
        act.Should().Throw<JsonException>()
            .WithMessage("Invalid JSON value for Boolean.");
    }

    [Fact]
    public void Write_ShouldWriteBooleanTrueAsString() {
        // Arrange: Prepare a boolean true value.
        var converter = new StringifiedBooleanConverter();
        const bool value = true;
        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();

        // Act: Write the boolean value using the converter.
        using (var writer = new Utf8JsonWriter(stream)) {
            converter.Write(writer, value, options);
        }

        var jsonOutput = Encoding.UTF8.GetString(stream.ToArray());

        // Assert: The output should be the string representation "True" wrapped in quotes.
        const string expectedOutput = "\"True\"";
        jsonOutput.Should().Be(expectedOutput);
    }

    [Fact]
    public void Write_ShouldWriteBooleanFalseAsString() {
        // Arrange: Prepare a boolean false value.
        var converter = new StringifiedBooleanConverter();
        const bool value = false;
        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();

        // Act: Write the boolean value using the converter.
        using (var writer = new Utf8JsonWriter(stream)) {
            converter.Write(writer, value, options);
        }

        var jsonOutput = Encoding.UTF8.GetString(stream.ToArray());

        // Assert: The output should be the string representation "False" wrapped in quotes.
        const string expectedOutput = "\"False\"";
        jsonOutput.Should().Be(expectedOutput);
    }
}