using System.Text;
using AwesomeAssertions;
using FrontierSharp.SuiClient;
using FrontierSharp.SuiClient.GraphQl;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.SuiClient;

public class SuiGraphQlClientTests {
    private const string ValidObjectsResponse = """
        {
          "data": {
            "objects": {
              "nodes": [
                {
                  "address": "0xabc123",
                  "asMoveObject": {
                    "contents": {
                      "json": {
                        "key": { "item_id": "100", "tenant": "0xtenant1" },
                        "killer_id": "200",
                        "victim_id": "300",
                        "reported_by_character_id": "400",
                        "kill_timestamp": "1700000000",
                        "loss_type": 1,
                        "solar_system_id": "500"
                      },
                      "type": {
                        "repr": "0xpkg::killmail::Killmail"
                      }
                    }
                  }
                }
              ],
              "pageInfo": {
                "hasNextPage": true,
                "endCursor": "eyJjIjoxfQ"
              }
            }
          }
        }
        """;

    private const string GraphQlErrorResponse = """
        {
          "data": null,
          "errors": [
            {
              "message": "Type not found",
              "locations": [{ "line": 1, "column": 1 }]
            }
          ]
        }
        """;

    private readonly MockLogger<SuiGraphQlClient> _logger = Substitute.For<MockLogger<SuiGraphQlClient>>();
    private readonly HybridCache _cache = new FakeHybridCache();
    private readonly IOptions<SuiClientOptions> _options = Substitute.For<IOptions<SuiClientOptions>>();

    public SuiGraphQlClientTests() {
        _options.Value.Returns(new SuiClientOptions {
            HttpClientName = "SuiGraphQl",
            GraphQlEndpoint = "https://test.local/graphql"
        });
    }

    [Fact]
    public async Task QueryAsync_ValidResponse_ReturnsData() {
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(ValidObjectsResponse);
        var client = new SuiGraphQlClient(factory, _cache, _options, _logger);

        var result = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType,
            new Dictionary<string, object?> { ["type"] = "0xpkg::killmail::Killmail", ["first"] = 10 });

        result.IsSuccess.Should().BeTrue();
        result.Value.Objects.Nodes.Should().HaveCount(1);
        result.Value.Objects.Nodes[0].Address.Should().Be("0xabc123");
        result.Value.Objects.PageInfo.HasNextPage.Should().BeTrue();
        result.Value.Objects.PageInfo.EndCursor.Should().Be("eyJjIjoxfQ");
    }

    [Fact]
    public async Task QueryAsync_LogsSerializedPayloadAtDebugLevel() {
        var cache = new FakeHybridCache();
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(ValidObjectsResponse);
        var client = new SuiGraphQlClient(factory, cache, _options, _logger);
        var variables = new Dictionary<string, object?> {
            ["type"] = "0xpkg::killmail::Killmail",
            ["first"] = 10
        };

        var result = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, variables);

        result.IsSuccess.Should().BeTrue();
        _logger.Received().Log(
            LogLevel.Debug,
            Arg.Is<IDictionary<string, object>>(state =>
                state.ContainsKey("Endpoint") &&
                Equals(state["Endpoint"], "https://test.local/graphql") &&
                state.ContainsKey("Payload") &&
                state["Payload"].ToString()!.Contains("\"query\":") &&
                state["Payload"].ToString()!.Contains("\"variables\":") &&
                state["Payload"].ToString()!.Contains("0xpkg::killmail::Killmail") &&
                state.ContainsKey("{OriginalFormat}") &&
                Equals(state["{OriginalFormat}"], "Sending GraphQL query to {Endpoint} with payload {Payload}")),
            Arg.Is<Exception?>(e => e == null));
    }

    [Fact]
    public async Task QueryAsync_GraphQlErrors_ReturnsFailure() {
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(GraphQlErrorResponse);
        var client = new SuiGraphQlClient(factory, _cache, _options, _logger);

        var result = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType,
            new Dictionary<string, object?> { ["type"] = "0xpkg::killmail::Killmail" });

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Type not found");
    }

    [Fact]
    public async Task QueryAsync_GraphQlErrors_DoesNotCacheFailure() {
        var callCount = 0;
        var cache = new FakeHybridCache();
        var factory = new SubstitutableHttpClientFactory((_, _) => {
            callCount++;
            return Task.FromResult(new HttpResponseMessage {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(GraphQlErrorResponse, Encoding.UTF8, "application/json")
            });
        });
        var client = new SuiGraphQlClient(factory, cache, _options, _logger);
        var variables = new Dictionary<string, object?> { ["type"] = "0xpkg::killmail::Killmail" };

        var firstResult = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, variables);
        var secondResult = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, variables);

        firstResult.IsFailed.Should().BeTrue();
        secondResult.IsFailed.Should().BeTrue();
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task QueryAsync_HttpError_ReturnsFailure() {
        var factory = SubstitutableHttpClientFactory.CreateInternalServerError();
        var client = new SuiGraphQlClient(factory, _cache, _options, _logger);

        var result = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType,
            new Dictionary<string, object?> { ["type"] = "0xpkg::killmail::Killmail" });

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("500");
    }

    [Fact]
    public async Task QueryAsync_EmptyDataResponse_ReturnsFailure() {
        const string emptyResponse = """{"data": null}""";
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(emptyResponse);
        var client = new SuiGraphQlClient(factory, _cache, _options, _logger);

        var result = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType,
            new Dictionary<string, object?> { ["type"] = "0xpkg::killmail::Killmail" });

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("no data");
    }

    [Fact]
    public async Task QueryAsync_MalformedJson_ReturnsFailure() {
        const string badJson = "not json at all";
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(badJson);
        var client = new SuiGraphQlClient(factory, _cache, _options, _logger);

        var result = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType,
            new Dictionary<string, object?> { ["type"] = "0xpkg::killmail::Killmail" });

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("deserialize");
    }

    [Fact]
    public async Task QueryAsync_LogsResponsePayloadAtDebugLevel() {
        const string badJson = "not json at all";
        var cache = new FakeHybridCache();
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(badJson);
        var client = new SuiGraphQlClient(factory, cache, _options, _logger);

        var result = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType,
            new Dictionary<string, object?> { ["type"] = "0xpkg::killmail::Killmail" });

        result.IsFailed.Should().BeTrue();
        _logger.Received().Log(
            LogLevel.Debug,
            Arg.Is<IDictionary<string, object>>(state =>
                state.ContainsKey("Endpoint") &&
                Equals(state["Endpoint"], "https://test.local/graphql") &&
                state.ContainsKey("Payload") &&
                Equals(state["Payload"], badJson) &&
                state.ContainsKey("{OriginalFormat}") &&
                Equals(state["{OriginalFormat}"], "Received GraphQL response from {Endpoint} with payload {Payload}")),
            Arg.Is<Exception?>(e => e == null));
    }

    [Fact]
    public async Task QueryAsync_IdenticalRequests_UsesCache() {
        var callCount = 0;
        var factory = new SubstitutableHttpClientFactory((_, _) => {
            callCount++;
            return Task.FromResult(new HttpResponseMessage {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(ValidObjectsResponse, Encoding.UTF8, "application/json")
            });
        });
        var client = new SuiGraphQlClient(factory, _cache, _options, _logger);
        var variables = new Dictionary<string, object?> {
            ["type"] = "0xpkg::killmail::Killmail",
            ["first"] = 10
        };

        var firstResult = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, variables);
        var secondResult = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, variables);

        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task QueryAsync_BypassCache_HitsTransportForEachRequest() {
        var callCount = 0;
        var factory = new SubstitutableHttpClientFactory((_, _) => {
            callCount++;
            return Task.FromResult(new HttpResponseMessage {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(ValidObjectsResponse, Encoding.UTF8, "application/json")
            });
        });
        var client = new SuiGraphQlClient(factory, _cache, _options, _logger);
        var variables = new Dictionary<string, object?> {
            ["type"] = "0xpkg::killmail::Killmail",
            ["first"] = 10
        };
        var queryOptions = new GraphQlQueryOptions {
            BypassCache = true
        };

        var firstResult = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, variables, queryOptions);
        var secondResult = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, variables, queryOptions);

        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task QueryAsync_SameVariablesDifferentOrder_UsesSameCacheEntry() {
        var callCount = 0;
        var factory = new SubstitutableHttpClientFactory((_, _) => {
            callCount++;
            return Task.FromResult(new HttpResponseMessage {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(ValidObjectsResponse, Encoding.UTF8, "application/json")
            });
        });
        var client = new SuiGraphQlClient(factory, _cache, _options, _logger);
        var firstVariables = new Dictionary<string, object?> {
            ["type"] = "0xpkg::killmail::Killmail",
            ["first"] = 10,
            ["after"] = null
        };
        var secondVariables = new Dictionary<string, object?> {
            ["after"] = null,
            ["first"] = 10,
            ["type"] = "0xpkg::killmail::Killmail"
        };

        var firstResult = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, firstVariables);
        var secondResult = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, secondVariables);

        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task QueryAsync_DifferentAfterCursor_UsesDifferentCacheEntries() {
        var callCount = 0;
        var factory = new SubstitutableHttpClientFactory((_, _) => {
            callCount++;
            return Task.FromResult(new HttpResponseMessage {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(ValidObjectsResponse, Encoding.UTF8, "application/json")
            });
        });
        var client = new SuiGraphQlClient(factory, _cache, _options, _logger);
        var firstVariables = new Dictionary<string, object?> {
            ["type"] = "0xpkg::killmail::Killmail",
            ["first"] = 10,
            ["after"] = "cursor-1"
        };
        var secondVariables = new Dictionary<string, object?> {
            ["type"] = "0xpkg::killmail::Killmail",
            ["first"] = 10,
            ["after"] = "cursor-2"
        };

        var firstResult = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, firstVariables);
        var secondResult = await client.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, secondVariables);

        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();
        callCount.Should().Be(2);
    }
}

