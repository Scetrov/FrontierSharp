using FluentResults;
using FrontierSharp.HttpClient;
using FrontierSharp.WorldApi.Models;
using FrontierSharp.WorldApi.RequestModel;
using Microsoft.Extensions.DependencyInjection;

namespace FrontierSharp.WorldApi;

public class WorldApiClient([FromKeyedServices(nameof(WorldApiClient))] IFrontierSharpHttpClient httpClient) : IWorldApiClient {
    public async Task<Result<GameType>> GetTypeById(long id, CancellationToken cancellationToken = default) {
        var requestModel = new GetTypeById {
            TypeId = id
        };
        var result = await httpClient.Get<GetTypeById, GameType>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<WorldApiPayload<GameType>>> GetTypesPage(long limit = 100, long offset = 0,
        CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfTypes {
            Limit = limit,
            Offset = offset
        };
        var result = await httpClient.Get<GetListOfTypes, WorldApiPayload<GameType>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<GameType>>> GetAllTypes(long limit = 100, CancellationToken cancellationToken = default) {
        return await GetAll(GetTypesPage, limit, cancellationToken);
    }

    public async Task<Result<SolarSystemDetail>> GetSolarSystemById(long id, CancellationToken cancellationToken = default) {
        var requestModel = new GetSolarSystemById {
            SolarSystemId = id
        };
        var result = await httpClient.Get<GetSolarSystemById, SolarSystemDetail>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<WorldApiPayload<SolarSystem>>> GetSolarSystemPage(long limit = 1000, long offset = 0,
        CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfSolarSystems {
            Limit = limit,
            Offset = offset
        };
        var result = await httpClient.Get<GetListOfSolarSystems, WorldApiPayload<SolarSystem>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<SolarSystem>>> GetAllSolarSystems(long limit = 1000, CancellationToken cancellationToken = default) {
        return await GetAll(GetSolarSystemPage, limit, cancellationToken);
    }


    // Tribes
    public async Task<Result<WorldApiPayload<Tribe>>>
        GetTribesPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfTribes {
            Limit = limit,
            Offset = offset
        };
        var result = await httpClient.Get<GetListOfTribes, WorldApiPayload<Tribe>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<Tribe>>> GetAllTribes(long limit = 100, CancellationToken cancellationToken = default) {
        return await GetAll(GetTribesPage, limit, cancellationToken);
    }

    public async Task<Result<TribeDetail>> GetTribeById(long id, CancellationToken cancellationToken = default) {
        var requestModel = new GetTribeById {
            TribeId = id
        };
        var result = await httpClient.Get<GetTribeById, TribeDetail>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    private static async Task<Result<IEnumerable<T>>> GetAll<T>(Func<long, long, CancellationToken, Task<Result<WorldApiPayload<T>>>> pageFunction,
        long limit = 100, CancellationToken cancellationToken = default) {
        var allItems = new List<T>();
        long offset = 0;
        var total = long.MaxValue;

        while (offset < total) {
            var result = await pageFunction(limit, offset, cancellationToken);
            if (result.IsFailed) return Result.Fail<IEnumerable<T>>(result.Errors);

            allItems.AddRange(result.Value.Data);
            total = result.Value.Metadata.Total;
            offset += limit;
        }

        return allItems;
    }
}