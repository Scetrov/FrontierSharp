using Xunit;
using FluentResults;
using FrontierSharp.Common.Utils;

namespace FrontierSharp.Tests.Common
{
    public class FluentResultsExtensionsTests
    {
        [Fact]
        public void ToErrorString_ReturnsEmpty_WhenSuccess()
        {
            var result = Result.Ok<int>(0);
            var s = result.ToErrorString();
            Assert.Equal(string.Empty, s);
        }

        [Fact]
        public void ToErrorString_FormatsSingleError()
        {
            var result = Result.Fail<int>("Something went wrong");
            var s = result.ToErrorString();
            Assert.Equal("- Something went wrong", s);
        }

        [Fact]
        public void ToErrorString_JoinsMultipleErrors_WithNewLines()
        {
            var result = Result.Fail<int>(new[] { "First", "Second" });
            var s = result.ToErrorString();
            var lines = s.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.None);
            Assert.Equal(2, lines.Length);
            Assert.Equal("- First", lines[0]);
            Assert.Equal("- Second", lines[1]);
        }
    }
}
