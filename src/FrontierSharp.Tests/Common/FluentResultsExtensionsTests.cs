using AwesomeAssertions;
using FluentResults;
using FrontierSharp.Common.Utils;
using Xunit;

namespace FrontierSharp.Tests.Common;

public class FluentResultsExtensionsTests {
    [Fact]
    public void ToErrorString_ReturnsEmpty_WhenSuccess() {
        var result = Result.Ok(0);
        var s = result.ToErrorString();
        s.Should().Be(string.Empty);
    }

    [Fact]
    public void ToErrorString_FormatsSingleError() {
        var result = Result.Fail<int>("Something went wrong");
        var s = result.ToErrorString();
        s.Should().Be("- Something went wrong");
    }

    [Fact]
    public void ToErrorString_JoinsMultipleErrors_WithNewLines() {
        var result = Result.Fail<int>(["First", "Second"]);
        var s = result.ToErrorString();
        var lines = s.Split([Environment.NewLine], StringSplitOptions.None);
        lines.Should().HaveCount(2);
        lines[0].Should().Be("- First");
        lines[1].Should().Be("- Second");
    }
}