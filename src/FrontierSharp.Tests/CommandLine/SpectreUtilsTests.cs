using Xunit;
using System.Numerics;
using FrontierSharp.CommandLine.Utils;
using System;

namespace FrontierSharp.Tests.CommandLine
{
    public class SpectreUtilsTests
    {
        [Fact]
        public void ToAnsiString_Bool_TrueFalse()
        {
            Assert.Equal("[green]Yes[/]", SpectreUtils.ToAnsiString(true));
            Assert.Equal("[red]No[/]", SpectreUtils.ToAnsiString(false));
        }

        [Fact]
        public void ToAnsiString_NullableBool_NullAndValues()
        {
            bool? n = null;
            Assert.Equal("[grey]N/A[/]", SpectreUtils.ToAnsiString(n));
            Assert.Equal("[green]Yes[/]", SpectreUtils.ToAnsiString((bool?)true));
            Assert.Equal("[red]No[/]", SpectreUtils.ToAnsiString((bool?)false));
        }

        [Fact]
        public void SliceMiddle_ReturnsSame_WhenShort()
        {
            var s = "short";
            Assert.Equal(s, s.SliceMiddle());
        }

        [Fact]
        public void SliceMiddle_TruncatesMiddle_WhenLong()
        {
            var s = "abcdefghijklmnopqrstuvwxyz"; // length 26
            var outStr = s.SliceMiddle(5); // should keep 5 from start and end
            Assert.Equal("abcde[grey]...[/]vwxyz", outStr);
        }

        [Fact]
        public void FuelToAnsiString_Boundaries()
        {
            Assert.Equal("[red]Empty[/]", SpectreUtils.FuelToAnsiString(0));

            var fifty = SpectreUtils.FuelToAnsiString(50);
            Assert.Contains("[orange1]50[/]", fifty);
            Assert.Contains("(", fifty);

            var hundred = SpectreUtils.FuelToAnsiString(100);
            Assert.Contains("[yellow]100[/]", hundred);

            var twoForty = SpectreUtils.FuelToAnsiString(240);
            Assert.Contains("[green]240[/]", twoForty);
        }

        [Fact]
        public void AsWeiToEther_FormatsCorrectly()
        {
            var wei = BigInteger.Parse("1000000000000000000");
            var s = SpectreUtils.AsWeiToEther(wei);
            Assert.Equal("1.00[grey]000[/]", s);
        }

        [Fact]
        public void Decimal_ToAnsiString_Format()
        {
            decimal d = 1.2345m;
            var s = SpectreUtils.ToAnsiString(d);
            // Expect numeric formatted with fixed pattern that includes the '[grey]' marker
            Assert.StartsWith("1.23", s);
            Assert.Contains("[grey]", s);
            Assert.Contains("[/]", s);
        }
    }
}
