using System.Numerics;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.Serialization;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api.Serialization;

public class BigIntegerConverterTests {
    [Fact]
    public void Read_WithStringDecimal_ShouldReturnBigIntegerTruncated() {
        // Arrange: A JSON string that represents a decimal number.
        // The fractional part will be truncated by BigInteger's constructor.
        var converter = new BigIntegerConverter();
        const string json = "\"123.456\"";
        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read(); // Advances to the token.

        // Act
        var result = converter.Read(ref reader, typeof(BigInteger), new JsonSerializerOptions());

        // Assert: 123.456 truncated to 123.
        result.Should().Be(new BigInteger(123));
    }

    [Fact]
    public void Read_WithStringBigInteger_ShouldReturnBigInteger() {
        // Arrange: A JSON string that represents a very large integer.
        // This value is too large to be parsed as a decimal.
        var converter = new BigIntegerConverter();
        const string largeNumber = "\"100000000000000000000000000000\""; // 30-digit number
        var bytes = Encoding.UTF8.GetBytes(largeNumber);
        var reader = new Utf8JsonReader(bytes);
        reader.Read();

        // Act
        var result = converter.Read(ref reader, typeof(BigInteger), new JsonSerializerOptions());

        // Assert: The value should be parsed via BigInteger.TryParse.
        var expected = BigInteger.Parse("100000000000000000000000000000");
        result.Should().Be(expected);
    }

    [Fact]
    public void Read_WithNumberToken_ShouldReturnBigInteger() {
        // Arrange: A JSON number token.
        var converter = new BigIntegerConverter();
        const string json = "123456789"; // JSON number (no quotes)
        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read(); // Advances to the Number token.

        // Act
        var result = converter.Read(ref reader, typeof(BigInteger), new JsonSerializerOptions());

        // Assert
        result.Should().Be(new BigInteger(123456789));
    }

    [Fact]
    public void Read_WithInvalidToken_ShouldThrowJsonException() {
        // Arrange: the JSON boolean "true" is an invalid value for BigInteger.
        var converter = new BigIntegerConverter();
        const string json = "true";

        // Act: Create the reader inside the lambda to avoid capturing a ref local.
        var act = () => {
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read(); // Advance to the Boolean token.
            converter.Read(ref reader, typeof(BigInteger), new JsonSerializerOptions());
        };

        // Assert: Expect a JsonException with the specified error message.
        act.Should().Throw<JsonException>()
            .WithMessage("Invalid JSON value for BigInteger.");
    }

    [Fact]
    public void Write_ShouldWriteBigIntegerAsString() {
        // Arrange: Prepare a BigInteger value.
        var converter = new BigIntegerConverter();
        var value = BigInteger.Parse("12345678901234567890");
        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();

        // Act: Write the BigInteger to the Utf8JsonWriter.
        using (var writer = new Utf8JsonWriter(stream)) {
            converter.Write(writer, value, options);
        }

        var jsonOutput = Encoding.UTF8.GetString(stream.ToArray());

        // Assert: The value should be written as a JSON string.
        jsonOutput.Should().Be("\"12345678901234567890\"");
    }
}