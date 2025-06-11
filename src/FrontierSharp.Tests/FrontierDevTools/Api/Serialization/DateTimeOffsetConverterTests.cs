using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using FrontierSharp.FrontierDevTools.Api.Serialization;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api.Serialization;

public class DateTimeOffsetConverterTests {
    [Fact]
    public void Read_WithValidDateTimeString_ShouldReturnParsedDateTimeOffset() {
        // Arrange: Provide a valid DateTimeOffset string.
        var converter = new DateTimeOffsetConverter();
        const string json = "\"2023-03-01T12:34:56+00:00\"";
        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read(); // Advance to the string token.

        // Act
        var result = converter.Read(ref reader, typeof(DateTimeOffset), new JsonSerializerOptions());

        // Assert: Compare against the expected DateTimeOffset value.
        var expected = DateTimeOffset.Parse("2023-03-01T12:34:56+00:00");
        result.Should().Be(expected);
    }

    [Fact]
    public void Read_WithValidUnixTimestampString_ShouldReturnDateTimeOffsetFromUnixTime() {
        // Arrange: Provide a valid long string representing a Unix timestamp.
        var converter = new DateTimeOffsetConverter();
        const string json = "\"1234567890\"";
        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read(); // Advance to the string token.

        // Act
        var result = converter.Read(ref reader, typeof(DateTimeOffset), new JsonSerializerOptions());

        // Assert: The Unix timestamp 1234567890 converted to DateTimeOffset.
        var expected = DateTimeOffset.FromUnixTimeSeconds(1234567890);
        result.Should().Be(expected);
    }

    [Fact]
    public void Read_WithNumberUnixTimestamp_ShouldReturnDateTimeOffsetFromUnixTime() {
        // Arrange: Provide a JSON number token representing a Unix timestamp.
        var converter = new DateTimeOffsetConverter();
        const string json = "1234567890"; // Number, not a string.
        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read(); // Advance to the number token.

        // Act
        var result = converter.Read(ref reader, typeof(DateTimeOffset), new JsonSerializerOptions());

        // Assert: The Unix timestamp 1234567890 converted to DateTimeOffset.
        var expected = DateTimeOffset.FromUnixTimeSeconds(1234567890);
        result.Should().Be(expected);
    }

    [Fact]
    public void Read_WithInvalidToken_ShouldThrowJsonException() {
        // Arrange: Use an invalid JSON token (a boolean) for DateTimeOffset.
        var converter = new DateTimeOffsetConverter();
        const string json = "true";

        // Act: Create the reader inside the lambda to avoid capturing a ref local.
        var act = () => {
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read(); // Advance to the boolean token.
            converter.Read(ref reader, typeof(DateTimeOffset), new JsonSerializerOptions());
        };

        // Assert: Expect a JsonException with the specified error message.
        act.Should().Throw<JsonException>()
            .WithMessage("Invalid JSON value for DateTimeOffset.");
    }

    [Fact]
    public void Write_ShouldWriteDateTimeOffsetAsString() {
        // Arrange: Prepare a DateTimeOffset value.
        var converter = new DateTimeOffsetConverter();
        var dateTimeOffset = DateTimeOffset.Parse("2023-03-01T12:34:56+00:00");
        const string expectedOutput = "\"1677674096000\"";

        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream)) {
            converter.Write(writer, dateTimeOffset, new JsonSerializerOptions());
        }

        var jsonOutput = Encoding.UTF8.GetString(stream.ToArray());

        // Assert: The output should equal the expected string.
        jsonOutput.Should().Be(expectedOutput);
    }
}