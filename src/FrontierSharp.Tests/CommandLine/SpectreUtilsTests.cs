using System.Numerics;
using FrontierSharp.CommandLine.Utils;
using Xunit;

namespace FrontierSharp.Tests.CommandLine;

public class SpectreUtilsTests {
    [Fact]
    public void ToAnsiString_Bool_TrueFalse() {
        Assert.Equal("[green]Yes[/]", true.ToAnsiString());
        Assert.Equal("[red]No[/]", false.ToAnsiString());
    }

    [Fact]
    public void ToAnsiString_NullableBool_NullAndValues() {
        bool? n = null;
        Assert.Equal("[grey]N/A[/]", n.ToAnsiString());
        Assert.Equal("[green]Yes[/]", ((bool?)true).ToAnsiString());
        Assert.Equal("[red]No[/]", ((bool?)false).ToAnsiString());
    }

    [Fact]
    public void SliceMiddle_ReturnsSame_WhenShort() {
        var s = "short";
        Assert.Equal(s, s.SliceMiddle());
    }

    [Fact]
    public void SliceMiddle_TruncatesMiddle_WhenLong() {
        var s = "abcdefghijklmnopqrstuvwxyz"; // length 26
        var outStr = s.SliceMiddle(5); // should keep 5 from start and end
        Assert.Equal("abcde[grey]...[/]vwxyz", outStr);
    }

    [Fact]
    public void FuelToAnsiString_Boundaries() {
        Assert.Equal("[red]Empty[/]", 0.FuelToAnsiString());

        var fifty = 50.FuelToAnsiString();
        Assert.Contains("[orange1]50[/]", fifty);
        Assert.Contains("(", fifty);

        var hundred = 100.FuelToAnsiString();
        Assert.Contains("[yellow]100[/]", hundred);

        var twoForty = 240.FuelToAnsiString();
        Assert.Contains("[green]240[/]", twoForty);
    }

    [Fact]
    public void AsWeiToEther_FormatsCorrectly() {
        var wei = BigInteger.Parse("1000000000000000000");
        var s = wei.AsWeiToEther();
        Assert.Equal("1.00[grey]000[/]", s);
    }

    [Fact]
    public void Decimal_ToAnsiString_Format() {
        var d = 1.2345m;
        var s = d.ToAnsiString();
        // Expect numeric formatted with fixed pattern that includes the '[grey]' marker
        Assert.StartsWith("1.23", s);
        Assert.Contains("[grey]", s);
        Assert.Contains("[/]", s);
    }
}