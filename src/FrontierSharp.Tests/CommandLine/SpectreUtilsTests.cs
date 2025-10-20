using System.Numerics;
using AwesomeAssertions;
using FrontierSharp.CommandLine.Utils;
using Xunit;

namespace FrontierSharp.Tests.CommandLine;

public class SpectreUtilsTests {
    [Fact]
    public void ToAnsiString_Bool_TrueFalse() {
        true.ToAnsiString().Should().Be("[green]Yes[/]");
        false.ToAnsiString().Should().Be("[red]No[/]");
    }

    [Fact]
    public void ToAnsiString_NullableBool_NullAndValues() {
        bool? n = null;
        n.ToAnsiString().Should().Be("[grey]N/A[/]");
        ((bool?)true).ToAnsiString().Should().Be("[green]Yes[/]");
        ((bool?)false).ToAnsiString().Should().Be("[red]No[/]");
    }

    [Fact]
    public void SliceMiddle_ReturnsSame_WhenShort() {
        var s = "short";
        s.SliceMiddle().Should().Be(s);
    }

    [Fact]
    public void SliceMiddle_TruncatesMiddle_WhenLong() {
        var s = "abcdefghijklmnopqrstuvwxyz"; // length 26
        var outStr = s.SliceMiddle(5); // should keep 5 from start and end
        outStr.Should().Be("abcde[grey]...[/]vwxyz");
    }

    [Fact]
    public void FuelToAnsiString_Boundaries() {
        0.FuelToAnsiString().Should().Be("[red]Empty[/]");

        var fifty = 50.FuelToAnsiString();
        fifty.Should().Contain("[orange1]50[/]");
        fifty.Should().Contain("(");

        var hundred = 100.FuelToAnsiString();
        hundred.Should().Contain("[yellow]100[/]");

        var twoForty = 240.FuelToAnsiString();
        twoForty.Should().Contain("[green]240[/]");
    }

    [Fact]
    public void AsWeiToEther_FormatsCorrectly() {
        var wei = BigInteger.Parse("1000000000000000000");
        var s = wei.AsWeiToEther();
        s.Should().Be("1.00[grey]000[/]");
    }

    [Fact]
    public void Decimal_ToAnsiString_Format() {
        var d = 1.2345m;
        var s = d.ToAnsiString();
        // Expect numeric formatted with fixed pattern that includes the '[grey]' marker
        s.Should().StartWith("1.23");
        s.Should().Contain("[grey]");
        s.Should().Contain("[/]");
    }
}