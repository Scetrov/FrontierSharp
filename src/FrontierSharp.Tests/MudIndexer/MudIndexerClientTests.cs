using AwesomeAssertions;
using FrontierSharp.MudIndexer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace FrontierSharp.Tests.MudIndexer;

public class MudIndexerClientTests {
    private const string SampleQuery = "SELECT * FROM test";
    private const string BaseUrl = "https://mud.local";
    private const string WorldAddress = "0xworld";
    private const string Endpoint = "/query";

    private static IOptions<MudIndexerOptions> CreateOptions() {
        return Options.Create(new MudIndexerOptions {
            IndexerAddress = BaseUrl,
            WorldAddress = WorldAddress,
            QueryEndpoint = Endpoint
        });
    }

    [Fact]
    public async Task Query_ShouldReturnParsedResult_WhenResponseIsValid() {
        // Arrange
        const string json = """
                            {
                                "block_height": 123456,
                                "result": [
                                    [
                                        ["id", "value"],
                                        ["1", "foo"],
                                        ["2", "bar"]
                                    ]
                                ]
                            }
                            """;

        var factory = SubstitutableHttpClientFactory.CreateWithPayload(json);
        var logger = new TestLogger();
        var sut = new MudIndexerClient(logger, factory, CreateOptions());

        // Act
        var responseResult = await sut.Query(SampleQuery);
        responseResult.IsSuccess.Should().BeTrue();
        var response = responseResult.Value;


        // Assert
        response.BlockHeight.Should().Be(123456);
        response.Result.Should().ContainSingle();
        var result = response.Result.First();
        result.Headers.Should().BeEquivalentTo("id", "value");
        result.Rows.Should().HaveCount(2);
        result.Rows.First().Should().BeEquivalentTo(["1", "foo"]);
        result.Rows.Last().Should().BeEquivalentTo(["2", "bar"]);
    }

    [Fact]
    public async Task Query_ShouldHandleEmptyResultArray() {
        // Arrange
        const string json = """
                            {
                                "block_height": 789000,
                                "result": []
                            }
                            """;

        var factory = SubstitutableHttpClientFactory.CreateWithPayload(json);
        var logger = new TestLogger();
        var sut = new MudIndexerClient(logger, factory, CreateOptions());

        // Act
        var responseResult = await sut.Query(SampleQuery);
        var response = responseResult.Value;

        // Assert
        response.BlockHeight.Should().Be(789000);
        response.Result.Should().BeEmpty();
    }

    [Fact]
    public async Task Query_ShouldHandleMissingResultKey() {
        // Arrange
        const string json = """
                            {
                                "block_height": 101010
                            }
                            """;

        var factory = SubstitutableHttpClientFactory.CreateWithPayload(json);
        var logger = new TestLogger();
        var sut = new MudIndexerClient(logger, factory, CreateOptions());

        // Act
        var responseResult = await sut.Query(SampleQuery);
        var response = responseResult.Value;

        // Assert
        response.BlockHeight.Should().Be(101010);
        response.Result.Should().BeEmpty();
    }

    [Fact]
    public async Task Query_ShouldHandleNullElements() {
        // Arrange
        const string json = """
                            {
                                "block_height": 500,
                                "result": [
                                    [
                                        ["id", "value"],
                                        ["1", null],
                                        ["2", "bar"]
                                    ]
                                ]
                            }
                            """;

        var factory = SubstitutableHttpClientFactory.CreateWithPayload(json);
        var logger = new TestLogger();
        var sut = new MudIndexerClient(logger, factory, CreateOptions());

        // Act
        var responseResult = await sut.Query(SampleQuery);
        var response = responseResult.Value;

        // Assert
        response.BlockHeight.Should().Be(500);
        var row1 = response.Result.First().Rows.ElementAt(0);
        var row2 = response.Result.First().Rows.ElementAt(1);

        row1.Should().BeEquivalentTo(["1", null]);
        row2.Should().BeEquivalentTo(["2", "bar"]);
    }

    [Fact]
    public async Task Query_ShouldHandleUnexpectedJsonStructure_Gracefully() {
        // Arrange
        const string invalidJson = """
                                   {
                                       "block_height": 999,
                                       "result": [
                                           [ "this", "is", "not", "a", "table" ]
                                       ]
                                   }
                                   """;

        var factory = SubstitutableHttpClientFactory.CreateWithPayload(invalidJson);
        var logger = new TestLogger();
        var sut = new MudIndexerClient(logger, factory, CreateOptions());

        // Act
        var responseResult = await sut.Query(SampleQuery);
        var response = responseResult.Value;

        // Assert
        response.BlockHeight.Should().Be(999);
        response.Result.Should().BeEmpty(); // Because result[0] is not a valid table
    }

    private class TestLogger : MockLogger<MudIndexerClient> {
        public override IDisposable BeginScope<TState>(TState state) {
            return null!;
        }

        public override void Log() {
        }

        public override void Log(LogLevel logLevel, string message, Exception? exception = null) {
        }

        public override void Log(LogLevel logLevel, IDictionary<string, object> state, Exception? exception = null) {
        }
    }
}