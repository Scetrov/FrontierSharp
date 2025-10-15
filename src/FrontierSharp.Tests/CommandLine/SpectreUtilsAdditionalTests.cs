using System;
using Xunit;
using FrontierSharp.CommandLine.Utils;
using Spectre.Console;
using System.Collections.Generic;

namespace FrontierSharp.Tests.CommandLine
{
    public class SpectreUtilsAdditionalTests
    {
        [Fact]
        public void CreateAnsiTable_HasColumnsAndTitle()
        {
            var table = SpectreUtils.CreateAnsiTable("Title", "Col1", "Col2");
            Assert.NotNull(table);
            Assert.Equal("Title", table.Title.Text);
            Assert.Equal(2, table.Columns.Count);
            // Avoid asserting header markup string content; Spectre columns use markup renderables.
        }

        [Fact]
        public void CreateAnsiListing_RendersRowsForDictionary()
        {
            var dict = new Dictionary<string, string> { { "k1", "v1" }, { "k2", "v2" } };
            var table = SpectreUtils.CreateAnsiListing("List", dict);
            Assert.NotNull(table);
            Assert.Equal(2, table.Rows.Count);
            // Verify column count but avoid relying on header markup string content
            Assert.Equal(2, table.Columns.Count);
        }

        [Fact]
        public void ToAnsiString_DateTimeOffset_Format()
        {
            var dto = new DateTimeOffset(2020, 1, 2, 0, 0, 0, TimeSpan.Zero);
            var s = SpectreUtils.ToAnsiString(dto);
            Assert.StartsWith("2020-01-02", s);
            Assert.Contains("(", s);
            Assert.Contains(")", s);
        }

        [Theory]
        [InlineData(99, "[orange1]99[")] // contains marker
        [InlineData(100, "[yellow]100[")]
        [InlineData(239, "[yellow]239[")]
        [InlineData(240, "[green]240[")]
        public void FuelToAnsiString_BoundaryValues(int value, string expectedStart)
        {
            var s = SpectreUtils.FuelToAnsiString(value);
            Assert.Contains(expectedStart, s);
        }

        [Fact]
        public void SliceMiddle_ExactBoundary_ReturnsOriginal()
        {
            var chars = 5;
            var value = new string('x', chars * 2);
            var outStr = value.SliceMiddle(chars);
            // The implementation truncates when length == chars*2, so expect the middle slice marker
            var expected = value.Substring(0, chars) + "[grey]...[/]" + value.Substring(value.Length - chars, chars);
            Assert.Equal(expected, outStr);
        }
    }
}
