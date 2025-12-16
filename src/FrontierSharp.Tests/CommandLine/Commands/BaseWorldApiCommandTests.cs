using AwesomeAssertions;
using FluentResults;
using FrontierSharp.CommandLine;
using FrontierSharp.CommandLine.Commands;
using FrontierSharp.WorldApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Spectre.Console;
using Spectre.Console.Cli;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Commands;

public class BaseWorldApiCommandTests {
    [Fact]
    public async Task LoadAllPagesAsync_AggregatesPages() {
        var client = Substitute.For<IWorldApiClient>();
        var console = Substitute.For<IAnsiConsole>();
        var logger = Substitute.For<ILogger<BaseWorldApiCommand<BaseWorldApiSettings>>>();
        var options = Options.Create(new ConfigurationOptions());

        var cmd = new TestCommand(logger, client, console, options);

        // Simulate two pages yielding total 3 items
        Task<Result<WorldApiPayload<int>>> PageFunc(long limit, long offset, CancellationToken ct) {
            if (offset == 0)
                return Task.FromResult(Result.Ok(new WorldApiPayload<int>
                    { Data = [1, 2], Metadata = new WorldApiMetadata { Total = 3, Limit = limit, Offset = offset } }));
            return Task.FromResult(Result.Ok(new WorldApiPayload<int>
                { Data = [3], Metadata = new WorldApiMetadata { Total = 3, Limit = limit, Offset = offset } }));
        }

        var res = await cmd.InvokeLoadAllPages(PageFunc, 2, CancellationToken.None);
        res.IsSuccess.Should().BeTrue();
        res.Value.Count.Should().Be(3);
    }

    [Fact]
    public void BuildFuzzyCandidates_ReturnsClosestGroup() {
        var client = Substitute.For<IWorldApiClient>();
        var console = Substitute.For<IAnsiConsole>();
        var logger = Substitute.For<ILogger<BaseWorldApiCommand<BaseWorldApiSettings>>>();
        var options = Options.Create(new ConfigurationOptions());

        var cmd = new TestCommand(logger, client, console, options);
        // Use a concrete type so we can assert on the Name property
        var items = new[] { new Item { Name = "Alpha" }, new Item { Name = "Alfa" }, new Item { Name = "Beta" } };
        var candidates = cmd.InvokeBuildFuzzyCandidates(items, "Alf", x => x.Name);
        candidates.Should().NotBeEmpty();
        candidates.Select(x => x.Value.Name).Should().Contain("Alfa");
    }

    private class TestCommand(ILogger logger, IWorldApiClient client, IAnsiConsole console, IOptions<ConfigurationOptions> options)
        : BaseWorldApiCommand<BaseWorldApiSettings>(logger, client, console, options) {

        // Implement abstract ExecuteAsync to satisfy AsyncCommand contract for tests
        public override Task<int> ExecuteAsync(CommandContext context, BaseWorldApiSettings settings, CancellationToken cancellationToken) {
            return Task.FromResult(0);
        }

        public Task<Result<List<T>>> InvokeLoadAllPages<T>(Func<long, long, CancellationToken, Task<Result<WorldApiPayload<T>>>> pageFunc,
            long pageSize, CancellationToken ct) {
            return LoadAllPagesAsync(pageFunc, pageSize, ct);
        }

        public IReadOnlyList<(T Value, int Distance)> InvokeBuildFuzzyCandidates<T>(IEnumerable<T> items, string name, Func<T, string> nameSelector) {
            var internalCandidates = BuildFuzzyCandidates(items, name, nameSelector);
            return internalCandidates.Select(c => (c.Value, c.Distance)).ToList();
        }
    }

    private class Item {
        public string Name { get; set; } = string.Empty;
    }
}