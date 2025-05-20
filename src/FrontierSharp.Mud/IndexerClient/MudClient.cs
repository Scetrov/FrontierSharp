using System.Net.Http.Json;
using System.Text.Json.Nodes;
using FrontierSharp.Mud.Linq;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace FrontierSharp.Mud.IndexerClient;

public class MudClient(
    IHttpClientFactory clientFactory,
    HybridCache cache,
    IOptions<IndexerClientOptions> options) {
    public async Task<IEnumerable<T>> Query<T>(string query, CancellationToken cancellationToken = default) {
        var chainOptions = options.Value;
        QueryObj[] payload = [new() { Address = chainOptions.StoreAddress, Query = query }];
        return await cache.GetOrCreateAsync($"{chainOptions.StoreAddress}:{query}", async (ct) => {
            var client = clientFactory.CreateClient("MudIndexer");
            var response = await client.PostAsync(chainOptions.QueryEndpoint, JsonContent.Create(payload), ct);

            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException(
                    $"The query `{query}` failed with status code {response.StatusCode} and message: \n`{errorContent}`");
            }

            var content = await response.Content.ReadAsStreamAsync(ct);
            var node = await JsonNode.ParseAsync(content, cancellationToken: ct);

            if (node?["result"] is not JsonArray resultTables) {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException(
                    $"The query `{query}` returned an unexpected payload: \n`{errorContent}`");
            }
            
            if (resultTables.Count == 0 || resultTables[0] is not JsonArray || resultTables[0]?.AsArray().Count == 0) {
                return [];
            }

            var data = resultTables[0]?.AsArray()
                .Where(x => x is JsonArray)
                .ToArray();

            if (data is null || data.Length <= 1) {
                return [];
            }
            
            foreach (var row in data.Skip(1)) {
                var instance = Activator.CreateInstance<T>();
                if (row is not JsonArray rowArray) {
                    throw new InvalidOperationException(
                        "Unable to parse the row, unexpected payload.");
                }
            }
            

            return Enumerable.Empty<T>();
        }, cancellationToken: cancellationToken);
    }

    private struct QueryObj {
        public string Address { get; set; }

        public string Query { get; set; }
    }
}