using AwesomeAssertions;
using FluentResults;
using FrontierSharp.CommandLine;
using FrontierSharp.CommandLine.Commands;
using FrontierSharp.WorldApi;
using FrontierSharp.WorldApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Commands;

public class GetTribeCommandSettingsTests {
    [Fact]
    public void Validate_Fails_WhenNoOptionSpecified() {
        var settings = new GetTribeCommand.Settings();
        var result = settings.Validate();
        result.Successful.Should().BeFalse();
    }

    [Fact]
    public void Validate_Succeeds_WhenIdSpecified() {
        var settings = new GetTribeCommand.Settings { Id = 1 };
        var result = settings.Validate();
        result.Successful.Should().BeTrue();
    }

    [Fact]
    public void Validate_Succeeds_WhenNameSpecified() {
        var settings = new GetTribeCommand.Settings { Name = "Sky" };
        var result = settings.Validate();
        result.Successful.Should().BeTrue();
    }

    [Fact]
    public void Validate_Fails_WhenMultipleOptionsSpecified() {
        var settings = new GetTribeCommand.Settings { Id = 1, Name = "Sky" };
        var result = settings.Validate();
        result.Successful.Should().BeFalse();
    }
}

public class GetTribeCommandTests {
    private static GetTribeCommand CreateCommand(
        IWorldApiClient? worldApiClient = null,
        IAnsiConsole? ansiConsole = null,
        ILogger<GetTribeCommand>? logger = null) {
        return new GetTribeCommand(
            logger ?? Substitute.For<ILogger<GetTribeCommand>>(),
            worldApiClient ?? Substitute.For<IWorldApiClient>(),
            ansiConsole ?? Substitute.For<IAnsiConsole>(),
            Options.Create(new ConfigurationOptions()));
    }

    [Fact]
    public async Task ExecuteAsync_ShowAll_WritesTable() {
        var worldApiClient = Substitute.For<IWorldApiClient>();
        worldApiClient.GetTribesPage(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new WorldApiPayload<Tribe> {
                Data = [
                    new Tribe { Id = 10, Name = "Alpha", MemberCount = 1, TaxRate = 5.5, FoundedAt = DateTimeOffset.UtcNow }
                ],
                Metadata = new WorldApiMetadata { Total = 1, Limit = 100, Offset = 0 }
            }));

        var console = Substitute.For<IAnsiConsole>();
        var command = CreateCommand(worldApiClient, console);
        var settings = new GetTribeCommand.Settings { ShowAll = true };
        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);

        exitCode.Should().Be(0);
        console.Received(1).Write(Arg.Any<Table>());
    }

    [Fact]
    public async Task ExecuteAsync_Id_ShowsDetails() {
        var detail = new TribeDetail {
            Id = 1,
            Name = "Zar",
            Members = [new TribeMember { Name = "Joe" }, new TribeMember { Name = "Mary" }]
        };

        var worldApiClient = Substitute.For<IWorldApiClient>();
        worldApiClient.GetTribeById(1, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(detail));

        var console = Substitute.For<IAnsiConsole>();
        var command = CreateCommand(worldApiClient, console);
        var settings = new GetTribeCommand.Settings { Id = 1, MembersLimit = 1 };
        await command.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);

        console.Received(1).Write(Arg.Any<Table>());
        await worldApiClient.Received().GetTribeById(1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Name_FuzzyWarnsAndGuides() {
        var worldApiClient = Substitute.For<IWorldApiClient>();
        worldApiClient.GetTribesPage(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new WorldApiPayload<Tribe> {
                Data = [
                    new Tribe { Id = 10, Name = "Alfa" },
                    new Tribe { Id = 12, Name = "Alfo" }
                ],
                Metadata = new WorldApiMetadata { Total = 2, Offset = 0, Limit = 100 }
            }));

        worldApiClient.GetTribeById(10, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new TribeDetail { Id = 10, Name = "Alfa" }));

        var console = Substitute.For<IAnsiConsole>();
        var command = CreateCommand(worldApiClient, console);
        var settings = new GetTribeCommand.Settings { Name = "Alf" };
        await command.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);

        // MarkupLine is an extension method that calls Write(IRenderable) internally.
        // Verify Write was called multiple times: for the "Multiple close matches" warning lines and the table.
        console.Received(4).Write(Arg.Any<IRenderable>()); // 1 warning + 2 candidate lines + 1 table
    }
}