using System.Numerics;
using FluentResults;
using FrontierSharp.HttpClient;
using FrontierSharp.WorldApi.Models;
using FrontierSharp.WorldApi.RequestModel;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FrontierSharp.WorldApi;

public class WorldApiClient([FromKeyedServices(nameof(WorldApiClient))] IFrontierSharpHttpClient httpClient) : IWorldApiClient {
    public async Task<Result<GameType>> GetTypeById(long id, CancellationToken cancellationToken = default) {
        var requestModel = new GetTypeById { TypeId = id };
        var result = await httpClient.Get<GetTypeById, GameType>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<WorldApiPayload<GameType>>> GetTypesPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfTypes { Limit = limit, Offset = offset };
        var result = await httpClient.Get<GetListOfTypes, WorldApiPayload<GameType>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<GameType>>> GetAllTypes(long limit = 100, CancellationToken cancellationToken = default) {
        return await GetAll(GetTypesPage, limit, cancellationToken);
    }

    public async Task<Result<WorldApiPayload<Fuel>>> GetFuelsPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfFuels { Limit = limit, Offset = offset };
        var result = await httpClient.Get<GetListOfFuels, WorldApiPayload<Fuel>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<Fuel>>> GetAllFuels(long limit = 100, CancellationToken cancellationToken = default) {
        return await GetAll(GetFuelsPage, limit, cancellationToken);
    }

    public async Task<Result<SolarSystemDetail>> GetSolarSystemById(long id, CancellationToken cancellationToken = default) {
        var requestModel = new GetSolarSystemById { SolarSystemId = id };
        var result = await httpClient.Get<GetSolarSystemById, SolarSystemDetail>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<WorldApiPayload<SolarSystem>>> GetSolarSystemPage(long limit = 1000, long offset = 0, CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfSolarSystems { Limit = limit, Offset = offset };
        var result = await httpClient.Get<GetListOfSolarSystems, WorldApiPayload<SolarSystem>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<SolarSystem>>> GetAllSolarSystems(long limit = 1000, CancellationToken cancellationToken = default) {
        return await GetAll(GetSolarSystemPage, limit, cancellationToken);
    }

    public async Task<Result<SmartAssemblyDetail>> GetSmartAssemblyById(BigInteger id, CancellationToken cancellationToken = default) {
        var requestModel = new GetSmartAssemblyById { SmartObjectId = id };
        var result = await httpClient.Get<GetSmartAssemblyById, SmartAssemblyDetail>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<WorldApiPayload<SmartAssemblyWithSolarSystem>>> GetSmartAssemblyPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfSmartAssemblies { Limit = limit, Offset = offset };
        var result = await httpClient.Get<GetListOfSmartAssemblies, WorldApiPayload<SmartAssemblyWithSolarSystem>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<SmartAssemblyWithSolarSystem>>> GetAllSmartAssemblies(long limit = 100, CancellationToken cancellationToken = default) {
        return await GetAll(GetSmartAssemblyPage, limit, cancellationToken);
    }

    public async Task<Result<SmartCharacterDetail>> GetSmartCharacterById(string address, CancellationToken cancellationToken = default) {
        var requestModel = new GetSmartCharacterById { CharacterAddress = address };
        var result = await httpClient.Get<GetSmartCharacterById, SmartCharacterDetail>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<WorldApiPayload<SmartCharacter>>> GetSmartCharacterPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfSmartCharacters { Limit = limit, Offset = offset };
        var result = await httpClient.Get<GetListOfSmartCharacters, WorldApiPayload<SmartCharacter>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<SmartCharacter>>> GetAllSmartCharacters(long limit = 100, CancellationToken cancellationToken = default) {
        return await GetAll(GetSmartCharacterPage, limit, cancellationToken);
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