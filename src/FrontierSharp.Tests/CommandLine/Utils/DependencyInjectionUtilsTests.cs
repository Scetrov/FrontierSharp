using AwesomeAssertions;
using FrontierSharp.CommandLine.Utils;
using Serilog.Events;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Utils;

public class DependencyInjectionUtilsTests {
    [Fact]
    public void GetLogLevel_ShouldReturnDefault_WhenNoFlagPresent() {
        var args = new[] {
            "--some-flag", "value"
        };

        var level = args.GetLogLevel(LogEventLevel.Information);

        level.Should().Be(LogEventLevel.Information);
    }

    [Theory]
    [InlineData("Verbose", LogEventLevel.Verbose)]
    [InlineData("Debug", LogEventLevel.Debug)]
    [InlineData("Information", LogEventLevel.Information)]
    [InlineData("Warning", LogEventLevel.Warning)]
    [InlineData("Error", LogEventLevel.Error)]
    [InlineData("Fatal", LogEventLevel.Fatal)]
    [InlineData("verbose", LogEventLevel.Verbose)]
    [InlineData("WARNING", LogEventLevel.Warning)]
    public void GetLogLevel_ShouldParseValidLogLevel_RegardlessOfCase(string input, LogEventLevel expected) {
        var args = new[] {
            "--loglevel", input
        };

        var level = args.GetLogLevel(LogEventLevel.Error);

        level.Should().Be(expected);
    }

    [Fact]
    public void GetLogLevel_ShouldReturnDefault_WhenLogLevelValueIsInvalid() {
        var args = new[] {
            "--loglevel", "nope"
        };

        var level = args.GetLogLevel(LogEventLevel.Fatal);

        level.Should().Be(LogEventLevel.Fatal);
    }

    [Fact]
    public void GetLogLevel_ShouldThrow_WhenLogLevelFlagIsLastArg() {
        var args = new[] {
            "--loglevel"
        };

        Action act = () => args.GetLogLevel();

        act.Should().Throw<IndexOutOfRangeException>();
    }
}