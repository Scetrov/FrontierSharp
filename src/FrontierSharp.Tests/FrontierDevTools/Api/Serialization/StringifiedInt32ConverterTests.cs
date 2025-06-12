using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using FrontierSharp.HttpClient.Serialization;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api.Serialization;

public class StringifiedInt32ConverterTests {
    [Fact]
    public void Read_WithValidString_ShouldReturnInt() {
        // Arrange: A JSON string representing a valid integer.
        var converter = new StringifiedInt32Converter();
        const string json = "\"123\"";
        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read(); // Advance to the string token.

        // Act
        var result = converter.Read(ref reader, typeof(int), new JsonSerializerOptions());

        // Assert
        result.Should().Be(123);
    }

    [Fact]
    public void Read_WithValidNumber_ShouldReturnInt() {
        // Arrange: A JSON number token representing a valid integer.
        var converter = new StringifiedInt32Converter();
        const string json = "123";
        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read(); // Advance to the number token.

        // Act
        var result = converter.Read(ref reader, typeof(int), new JsonSerializerOptions());

        // Assert
        result.Should().Be(123);
    }

    [Fact]
    public void Read_WithInvalidString_ShouldThrowJsonException() {
        // Arrange: A JSON string that cannot be parsed as an int.
        var converter = new StringifiedInt32Converter();
        const string json = "\"abc\"";
        var bytes = Encoding.UTF8.GetBytes(json);

        // Act: Create the reader inside the lambda to avoid capturing a ref local.
        var act = () => {
            var reader = new Utf8JsonReader(bytes);
            reader.Read();
            converter.Read(ref reader, typeof(int), new JsonSerializerOptions());
        };

        // Assert: Expect a JsonException with the specified error message.
        act.Should().Throw<JsonException>()
            .WithMessage("Invalid JSON value for Int32.");
    }

    [Fact]
    public void Read_WithInvalidToken_ShouldThrowJsonException() {
        // Arrange: A JSON boolean token is invalid for this converter.
        var converter = new StringifiedInt32Converter();
        const string json = "true"; // Boolean token, not a string or number.
        var bytes = Encoding.UTF8.GetBytes(json);

        // Act: Create the reader inside the lambda.
        var act = () => {
            var reader = new Utf8JsonReader(bytes);
            reader.Read();
            converter.Read(ref reader, typeof(int), new JsonSerializerOptions());
        };

        // Assert: Expect a JsonException with the specified error message.
        act.Should().Throw<JsonException>()
            .WithMessage("Invalid JSON value for Int32.");
    }

    [Fact]
    public void Write_ShouldWriteIntAsString() {
        // Arrange: Prepare an integer value.
        var converter = new StringifiedInt32Converter();
        const int value = 123;
        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();

        // Act: Write the integer value using the converter.
        using (var writer = new Utf8JsonWriter(stream)) {
            converter.Write(writer, value, options);
        }

        var jsonOutput = Encoding.UTF8.GetString(stream.ToArray());

        // Assert: The output should be the string representation "123" wrapped in quotes.
        const string expectedOutput = "\"123\"";
        jsonOutput.Should().Be(expectedOutput);
    }
}