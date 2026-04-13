using FluentResults;

namespace FrontierSharp.SuiClient.GraphQl;

public interface ISuiGraphQlClient {
    Task<Result<T>> QueryAsync<T>(string query, Dictionary<string, object?>? variables = null,
        CancellationToken cancellationToken = default) where T : class;
}

