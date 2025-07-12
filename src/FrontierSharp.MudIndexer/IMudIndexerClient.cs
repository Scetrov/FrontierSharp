using FluentResults;

namespace FrontierSharp.HttpClient;

public interface IMudIndexerClient {
    public Task<Result<IndexerResponse>> Query(string query, CancellationToken cancellationToken = default);
}