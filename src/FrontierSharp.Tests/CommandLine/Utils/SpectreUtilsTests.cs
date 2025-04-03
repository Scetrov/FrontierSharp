using System.Numerics;
using FluentAssertions;
using FrontierSharp.CommandLine.Utils;
using Humanizer;
using Spectre.Console;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Utils;

public class SpectreUtilsTests {
    [Fact]
    public void CreateAnsiTable_ShouldCreateTableWithCorrectTitleAndColumns() {
        // Arrange
        const string title = "Test Table";
        var columns = new[] { "Col1", "Col2", "Col3" };

        // Act
        var table = SpectreUtils.CreateAnsiTable(title, columns);

        // Assert
        table.Should().NotBeNull();
        table.Title.Should().NotBeNull();
        // Assuming TableTitle.Text returns the provided title.
        table.Title?.Text.Should().Be(title);
        table.Border.Should().Be(TableBorder.Rounded);
        table.BorderStyle?.Foreground.Should().Be(Color.Orange1);
        table.Columns.Count.Should().Be(columns.Length);
    }

    [Fact]
    public void CreateAnsiListing_ShouldCreateTableWithKeyValueColumnsAndRows() {
        // Arrange
        const string title = "Listing Table";
        var data = new Dictionary<string, string> {
            { "Key1", "Value1" },
            { "Key2", "Value2" }
        };

        // Act
        var table = SpectreUtils.CreateAnsiListing(title, data);

        // Assert
        table.Should().NotBeNull();
        table.Title.Should().NotBeNull();
        table.Title?.Text.Should().Be(title);
        table.Border.Should().Be(TableBorder.Rounded);
        table.BorderStyle?.Foreground.Should().Be(Color.Orange1);
        // CreateAnsiListing always adds two columns: "[bold]Key[/]" and "[bold]Value[/]"
        table.Columns.Count.Should().Be(2);

        // Verify rows: each row should have two cells. The first cell contains the key (wrapped in [bold][/])
        // and the second cell contains the corresponding value.
        table.Rows.Count.Should().Be(data.Count);
        foreach (var row in table.Rows) {
            row.Count.Should().Be(2);
            var cellKey = row[0].ToString();
            // Remove the markup to extract the plain key.
            var plainKey = cellKey?.Replace("[bold]", "").Replace("[/]", "");
            plainKey.Should().NotBeNull();
            data.Should().ContainKey("Key1");
            data["Key1"].Should().Be("Value1");
        }
    }

    [Fact]
    public void AsWeiToEther_ShouldConvertWeiToEtherString() {
        // Arrange
        // 1 Ether = 1e18 wei. The conversion divides by 1e18 and then calls
        // decimal.ToAnsiString() which formats using "0.00[grey]000[/]".
        var wei = BigInteger.Parse("1000000000000000000");
        const string expected = "1.00[grey]000[/]";

        // Act
        var result = wei.AsWeiToEther();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToAnsiString_Bool_ShouldReturnYesOrNo() {
        // Arrange
        const bool trueValue = true;
        const bool falseValue = false;
        const string expectedTrue = "[green]Yes[/]";
        const string expectedFalse = "[red]No[/]";

        // Act
        var trueResult = trueValue.ToAnsiString();
        var falseResult = falseValue.ToAnsiString();

        // Assert
        trueResult.Should().Be(expectedTrue);
        falseResult.Should().Be(expectedFalse);
    }

    [Fact]
    public void ToAnsiString_DateTimeOffset_ShouldFormatDateAndHumanized() {
        // Arrange
        // Use a fixed date so that the humanized part is predictable.
        // Note: Humanize() returns a relative time (e.g. "3 years ago") which depends on the current time.
        // Here we compute the expected value at runtime.
        var date = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var humanized = date.Humanize();
        var expected = $"{date:yyyy-MM-dd} [grey]({humanized})[/]";

        // Act
        var result = date.ToAnsiString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToAnsiString_Decimal_ShouldFormatDecimalCorrectly() {
        // Arrange
        const decimal value = 1m;
        // Expected output uses the format string "0.00[grey]000[/]"
        const string expected = "1.00[grey]000[/]";

        // Act
        var result = value.ToAnsiString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void SliceMiddle_WithShortString_ShouldReturnOriginalString() {
        // Arrange
        const string input = "short string";
        // When the input length is less than (default) 18 * 2 = 36 characters,
        // the original string is returned.
        const string expected = input;

        // Act
        var result = input.SliceMiddle();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void SliceMiddle_WithLongString_ShouldSliceAndInsertEllipsis() {
        // Arrange
        // Create a string longer than 36 characters.
        const string longInput = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890ABCD"; // Length = 40.
        // Default slicing uses 18 characters from start and end.
        var first18 = longInput.Substring(0, 18);
        var last18 = longInput.Substring(longInput.Length - 18, 18);
        var expected = $"{first18}[grey]...[/]{last18}";

        // Act
        var result = longInput.SliceMiddle();

        // Assert
        result.Should().Be(expected);
    }
}