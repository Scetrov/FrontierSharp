using System.Numerics;
using AwesomeAssertions;
using FrontierSharp.CommandLine.Utils;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Utils;

public class SpectreUtilsTests {
    [Fact]
    public void CreateAnsiTable_Should_Create_Table_With_Styled_Columns() {
        // Act
        var table = SpectreUtils.CreateAnsiTable("Test Title", "Col1", "Col2");

        // Assert
        table.Should().NotBeNull();
        table.Title!.Text.Should().Be("Test Title");
        table.Columns.Should().HaveCount(2);
    }

    [Fact]
    public void CreateAnsiListing_Should_Create_Table_With_KeyValueRows() {
        // Arrange
        var data = new Dictionary<string, string> {
            {
                "Key1", "Value1"
            }, {
                "Key2", "Value2"
            }
        };

        // Act
        var table = SpectreUtils.CreateAnsiListing("Listing", data);

        // Assert
        table.Should().NotBeNull();
        table.Title!.Text.Should().Be("Listing");
        table.Columns.Should().HaveCount(2);
        table.Rows.Should().HaveCount(2);
    }

    [Fact]
    public void AsWeiToEther_Should_Convert_And_Format_Correctly() {
        // Arrange
        var wei = BigInteger.Parse("1000000000000000000"); // 1 ETH

        // Act
        var result = wei.AsWeiToEther();

        // Assert
        result.Should().Be("1.00[grey]000[/]");
    }

    [Theory]
    [InlineData(true, "[green]Yes[/]")]
    [InlineData(false, "[red]No[/]")]
    public void ToAnsiString_Bool_Should_Return_Correct_Ansi(bool input, string expected) {
        // Act
        var result = input.ToAnsiString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "[green]Yes[/]")]
    [InlineData(false, "[red]No[/]")]
    public void ToAnsiString_NullableBool_Should_Return_Correct_Ansi_When_HasValue(bool input, string expected) {
        // Act
        var result = ((bool?)input).ToAnsiString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToAnsiString_NullableBool_Should_Return_NA_When_Null() {
        // Arrange
        bool? input = null;

        // Act
        var result = input.ToAnsiString();

        // Assert
        result.Should().Be("[grey]N/A[/]");
    }

    [Theory]
    [InlineData(0, "[red]Empty[/]")]
    [InlineData(50, "[orange1]50[/]")]
    [InlineData(150, "[yellow]150[/]")]
    [InlineData(300, "[green]300[/]")]
    public void FuelToAnsiString_Should_Format_Fuel_With_Color_And_Time(int value, string expectedStart) {
        // Act
        var result = value.FuelToAnsiString();

        // Assert
        result.Should().StartWith(expectedStart);
    }

    [Fact]
    public void ToAnsiString_DateTimeOffset_Should_Format_With_Date_And_HumanizedTime() {
        // Arrange
        var date = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1));

        // Act
        var result = date.ToAnsiString();

        // Assert
        result.Should().Contain(date.ToString("yyyy-MM-dd"));
        result.Should().Contain("[grey](yesterday)[/]");
    }

    [Theory]
    [InlineData(0.0001, "0.00[grey]010[/]")]
    [InlineData(123.456789, "123.45[grey]679[/]")]
    [InlineData(100, "100.00[grey]000[/]")]
    public void ToAnsiString_Decimal_Should_Format_With_GreyDecimalTail(decimal value, string expected) {
        // Act
        var result = value.ToAnsiString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void SliceMiddle_Should_Leave_Short_Strings_Unchanged() {
        // Arrange
        var input = "ShortString";

        // Act
        var result = input.SliceMiddle();

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void SliceMiddle_Should_Truncate_Long_Strings_With_Grey_Ellipsis() {
        // Arrange
        var input = new string('X', 50);

        // Act
        var result = input.SliceMiddle(10);

        // Assert
        result.Should().StartWith("XXXXXXXXXX[grey]...[/]");
        result.Should().EndWith("XXXXXXXXXX");
        result.Length.Should().BeLessThan(input.Length);
    }
}