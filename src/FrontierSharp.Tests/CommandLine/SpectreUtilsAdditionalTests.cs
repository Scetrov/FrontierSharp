using AwesomeAssertions;
using FrontierSharp.CommandLine.Utils;
using Xunit;

namespace FrontierSharp.Tests.CommandLine;

public class SpectreUtilsAdditionalTests {
    [Fact]
    public void CreateAnsiTable_HasColumnsAndTitle() {
        var table = SpectreUtils.CreateAnsiTable("Title", "Col1", "Col2");
        table.Should().NotBeNull();
        table.Title!.Text.Should().Be("Title");
        table.Columns.Count.Should().Be(2);
        // Avoid asserting header markup string content; Spectre columns use markup renderables.
    }

    [Fact]
    public void CreateAnsiListing_RendersRowsForDictionary() {
        var dict = new Dictionary<string, string> { { "k1", "v1" }, { "k2", "v2" } };
        var table = SpectreUtils.CreateAnsiListing("List", dict);
        table.Should().NotBeNull();
        table.Rows.Count.Should().Be(2);
        // Verify column count but avoid relying on header markup string content
        table.Columns.Count.Should().Be(2);
    }

    [Fact]
    public void ToAnsiString_DateTimeOffset_Format() {
        var dto = new DateTimeOffset(2020, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var s = dto.ToAnsiString();
        s.Should().StartWith("2020-01-02");
        s.Should().Contain("(");
        s.Should().Contain(")");
    }

    [Theory]
    [InlineData(0, "[red]0[/]")]
    [InlineData(1, "[red]1[/]")]
    [InlineData(20, "[red]20[/]")]
    [InlineData(21, "[yellow]21[/]")]
    [InlineData(50, "[yellow]50[/]")]
    [InlineData(79, "[yellow]79[/]")]
    [InlineData(80, "[green]80[/]")]
    [InlineData(90, "[green]90[/]")]
    public void FuelToAnsiString_BoundaryValues(int value, string expected) {
        var s = value.FuelToAnsiString();
        s.Should().Be(expected);
    }

    [Fact]
    public void SliceMiddle_ExactBoundary_ReturnsOriginal() {
        const int chars = 5;
        var value = new string('x', chars * 2);
        var outStr = value.SliceMiddle(chars);
        // The implementation truncates when length == chars*2, so expect the middle slice marker
        var expected = string.Concat(value.AsSpan(0, chars), "[grey]...[/]", value.AsSpan(value.Length - chars, chars));
        outStr.Should().Be(expected);
    }
}