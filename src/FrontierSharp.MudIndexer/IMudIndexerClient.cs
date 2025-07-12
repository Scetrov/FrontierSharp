using FluentResults;

namespace FrontierSharp.MudIndexer;

public interface IMudIndexerClient {
    public Task<Result<IndexerResponse>> Query(string query, CancellationToken cancellationToken = default);
}