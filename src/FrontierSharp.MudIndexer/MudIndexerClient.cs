using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FrontierSharp.HttpClient;

public class MudIndexerClient(ILogger<MudIndexerClient> logger, IHttpClientFactory clientFactory, IOptions<MudIndexerOptions> options) : IMudIndexerClient {
    private readonly MudIndexerOptions _options = options.Value;

    public async Task<Result<IndexerResponse>> Query(string query, CancellationToken cancellationToken = default) {
        IEnumerable<IndexerQuery> requestPlayload = [new() { Address = _options.WorldAddress, Query = query }];
        var uriBuilder = new UriBuilder(_options.IndexerAddress) {
            Path = _options.QueryEndpoint
        };
        var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString());
        request.Content = JsonContent.Create(requestPlayload);
        using var client = clientFactory.CreateClient(nameof(MudIndexerClient));
        var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode) {
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("MudIndexerClient: Query failed with status code {StatusCode} ({ReasonPhrase}) for query: {Query} and payload: {Payload}", response.StatusCode, response.ReasonPhrase, query, payload);
            return Result.Fail(new Error($"Query failed with status code {response.StatusCode}"));
        }

        var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var nodeResult = await ParseIndexerResponseAsync(content, cancellationToken);
        if (nodeResult.IsFailed) {
            logger.LogError("MudIndexerClient: Failed to parse response for query: {Query} with error: {Error}", query, nodeResult.Errors);
            return Result.Fail(nodeResult.Errors);
        }

        var node = nodeResult.Value;
        var queryResults = new List<QueryResult>();

        var payloadResults = node["result"]?.AsArray();

        if (payloadResults == null) {
            return new IndexerResponse {
                BlockHeight = node["block_height"]?.GetValue<long>() ?? 0,
                Result = queryResults
            };
        }

        foreach (var qr in payloadResults) {
            if (qr == null || qr.GetValueKind() != JsonValueKind.Array || qr.AsArray().Count == 0) {
                continue;
            }

            var queryResult = new QueryResult();

            if (qr.AsArray().Count == 0) {
                queryResults.Add(queryResult);
                continue;
            }

            var rows = qr.AsArray();

            if (qr.AsArray().Count == 0 || qr[0]!.GetValueKind() != JsonValueKind.Array) {
                continue;
            }

            queryResult.Headers = qr[0]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
            queryResult.Rows = rows.Skip(1).Select(x => x!.AsArray().Select(Parse).ToArray());
            queryResults.Add(queryResult);
        }

        return new IndexerResponse {
            BlockHeight = node["block_height"]?.GetValue<long>() ?? 0,
            Result = queryResults
        };
    }

    private static async Task<Result<JsonNode>> ParseIndexerResponseAsync(Stream content, CancellationToken cancellationToken) {
        try {
            var node = await JsonNode.ParseAsync(content, cancellationToken: cancellationToken);

            if (node == null) {
                return Result.Fail("Failed to parse JSON response as it yielded a null object");
            }

            return node;
        } catch (JsonException ex) {
            return Result.Fail("Failed to parse JSON response: " + ex.Message);
        }
    }

    private static object? Parse(JsonNode? element) {
        if (element == null) {
            return null;
        }

        if (element.GetValueKind() == JsonValueKind.String) {
            return element.GetValue<string>();
        }

        if (element.GetValueKind() == JsonValueKind.Null) {
            return null;
        }

        if (element.GetValueKind() == JsonValueKind.Number) {
            return element.GetValue<long>();
        }

        if (element.GetValueKind() == JsonValueKind.True) {
            return element.GetValue<bool>();
        }

        return element.Deserialize<object>();
    }
}