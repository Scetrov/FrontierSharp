using FluentAssertions;
using FrontierSharp.CommandLine.Commands;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Commands;

public class GetCorporationCommandSettingsTests {
    [Fact]
    public void Validate_ReturnsError_WhenNeitherIdNorPlayerProvided() {
        var settings = new GetCorporationCommand.Settings();

        var result = settings.Validate();

        result.Successful.Should().BeFalse();
        result.Message.Should().Contain("exactly one");
    }

    [Fact]
    public void Validate_ReturnsError_WhenBothIdAndPlayerProvided() {
        var settings = new GetCorporationCommand.Settings {
            Id = 42,
            PlayerName = "Alice"
        };

        var result = settings.Validate();

        result.Successful.Should().BeFalse();
        result.Message.Should().Contain("exactly one");
    }

    [Fact]
    public void Validate_Succeeds_WithOnlyId() {
        var settings = new GetCorporationCommand.Settings {
            Id = 42
        };

        var result = settings.Validate();

        result.Successful.Should().BeTrue();
    }

    [Fact]
    public void Validate_Succeeds_WithOnlyPlayerName() {
        var settings = new GetCorporationCommand.Settings {
            PlayerName = "Bob"
        };

        var result = settings.Validate();

        result.Successful.Should().BeTrue();
    }

    [Fact]
    public void SearchType_Returns_Id_WhenIdProvided() {
        var settings = new GetCorporationCommand.Settings {
            Id = 1
        };

        var result = settings.SearchType;

        result.Should().Be(GetCorporationCommand.CorporationSearchType.Id);
    }

    [Fact]
    public void SearchType_Returns_Player_WhenOnlyPlayerProvided() {
        var settings = new GetCorporationCommand.Settings {
            PlayerName = "Bob"
        };

        var result = settings.SearchType;

        result.Should().Be(GetCorporationCommand.CorporationSearchType.Player);
    }

    [Fact]
    public void SearchType_Throws_WhenNeitherIdNorPlayerProvided() {
        var settings = new GetCorporationCommand.Settings();

        Action act = () => _ = settings.SearchType;

        act.Should().Throw<NotImplementedException>()
           .WithMessage("Search Type not Implemented.");
    }
}
