using AwesomeAssertions;
using FrontierSharp.SuiClient.Models;
using Xunit;

namespace FrontierSharp.Tests.SuiClient;

public class CursorTests {
    [Fact]
    public void Constructor_StoresValueAndSupportsValueEquality() {
        var left = new Cursor("cursor123");
        var right = new Cursor("cursor123");

        left.Value.Should().Be("cursor123");
        left.Should().Be(right);
        left.ToString().Should().Be("cursor123");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_InvalidValue_ThrowsArgumentException(string value) {
        var act = () => new Cursor(value);

        act.Should().Throw<ArgumentException>();
    }
}

