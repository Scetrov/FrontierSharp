using FluentResults;
using FrontierSharp.HttpClient;
using FrontierSharp.WorldApi.Models;
using FrontierSharp.WorldApi.RequestModel;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FrontierSharp.WorldApi;

public class WorldApiClient(
    ILogger<WorldApiClient> logger,
    IHttpClientFactory httpClientFactory,
    HybridCache cache,
    IOptions<FrontierSharpHttpClientOptions> options)
    : FrontierSharpHttpClient(logger, httpClientFactory, cache, options), IWorldApiClient {

    public async Task<Result<GameType>> GetTypeById(long id, CancellationToken cancellationToken = default) {
        var requestModel = new GetTypeById { TypeId = id };
        var result = await Get<GetTypeById, GameType>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<WorldApiPayload<GameType>>> GetTypesPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfTypes { Limit = limit, Offset = offset };
        var result = await Get<GetListOfTypes, WorldApiPayload<GameType>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<GameType>>> GetAllTypes(long limit = 100, CancellationToken cancellationToken = default) {
        return await GetAll(GetTypesPage, limit, cancellationToken);
    }

    public async Task<Result<WorldApiPayload<Fuel>>> GetFuelsPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfFuels { Limit = limit, Offset = offset };
        var result = await Get<GetListOfFuels, WorldApiPayload<Fuel>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<Fuel>>> GetAllFuels(long limit = 100, CancellationToken cancellationToken = default) {
        return await GetAll(GetFuelsPage, limit, cancellationToken);
    }

    private async Task<Result<IEnumerable<T>>> GetAll<T>(Func<long, long, CancellationToken, Task<Result<WorldApiPayload<T>>>> pageFunction, long limit = 100, CancellationToken cancellationToken = default) {
        var allItems = new List<T>();
        long offset = 0;
        var total = long.MaxValue;

        while (offset < total) {
            var result = await pageFunction(limit, offset, cancellationToken);
            if (result.IsFailed) {
                return Result.Fail<IEnumerable<T>>(result.Errors);
            }

            allItems.AddRange(result.Value.Data);
            total = result.Value.Metadata.Total;
            offset += limit;
        }

        return allItems;
    }
}