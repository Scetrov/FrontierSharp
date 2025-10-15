using System.Numerics;
using FluentResults;
using FrontierSharp.HttpClient;
using FrontierSharp.WorldApi.Models;
using FrontierSharp.WorldApi.RequestModel;
using Microsoft.Extensions.DependencyInjection;

namespace FrontierSharp.WorldApi;

public class WorldApiClient([FromKeyedServices(nameof(WorldApiClient))] IFrontierSharpHttpClient httpClient) : IWorldApiClient {
    public async Task<Result<GameType>> GetTypeById(long id, string? format = null, CancellationToken cancellationToken = default) {
        var requestModel = new GetTypeById {
            TypeId = id,
            Format = format
        };
        var result = await httpClient.Get<GetTypeById, GameType>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<WorldApiPayload<GameType>>> GetTypesPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default) {
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

    public async Task<Result<WorldApiPayload<Fuel>>> GetFuelsPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfFuels {
            Limit = limit,
            Offset = offset
        };
        var result = await httpClient.Get<GetListOfFuels, WorldApiPayload<Fuel>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<Fuel>>> GetAllFuels(long limit = 100, CancellationToken cancellationToken = default) {
        return await GetAll(GetFuelsPage, limit, cancellationToken);
    }

    public async Task<Result<SolarSystemDetail>> GetSolarSystemById(long id, string? format = null, CancellationToken cancellationToken = default) {
        var requestModel = new GetSolarSystemById {
            SolarSystemId = id,
            Format = format
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

    public async Task<Result<SmartAssemblyDetail>> GetSmartAssemblyById(BigInteger id, string? format = null, CancellationToken cancellationToken = default) {
        var requestModel = new GetSmartAssemblyById {
            SmartObjectId = id,
            Format = format
        };
        var result = await httpClient.Get<GetSmartAssemblyById, SmartAssemblyDetail>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<WorldApiPayload<SmartAssemblyWithSolarSystem>>> GetSmartAssemblyPage(long limit = 100, long offset = 0,
        long? type = null, CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfSmartAssemblies {
            Limit = limit,
            Offset = offset,
            Type = type
        };
        var result = await httpClient.Get<GetListOfSmartAssemblies, WorldApiPayload<SmartAssemblyWithSolarSystem>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<SmartAssemblyWithSolarSystem>>>
        GetAllSmartAssemblies(long limit = 100, long? type = null, CancellationToken cancellationToken = default) {
        return await GetAll((l, o, ct) => GetSmartAssemblyPage(l, o, type, ct), limit, cancellationToken);
    }

    public async Task<Result<SmartCharacterDetail>> GetSmartCharacterById(string address, string? format = null, CancellationToken cancellationToken = default) {
        var requestModel = new GetSmartCharacterById {
            CharacterAddress = address,
            Format = format
        };
        var result = await httpClient.Get<GetSmartCharacterById, SmartCharacterDetail>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<WorldApiPayload<SmartCharacter>>> GetSmartCharacterPage(long limit = 100, long offset = 0,
        CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfSmartCharacters {
            Limit = limit,
            Offset = offset
        };
        var result = await httpClient.Get<GetListOfSmartCharacters, WorldApiPayload<SmartCharacter>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<SmartCharacter>>> GetAllSmartCharacters(long limit = 100, CancellationToken cancellationToken = default) {
        return await GetAll(GetSmartCharacterPage, limit, cancellationToken);
    }

    public async Task<Result<WorldApiPayload<Killmail>>> GetKillmailPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default) {
        var requestModel = new GetListOfKillmails {
            Limit = limit,
            Offset = offset
        };
        var result = await httpClient.Get<GetListOfKillmails, WorldApiPayload<Killmail>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<Killmail>>> GetAllKillmails(long limit = 100, CancellationToken cancellationToken = default) {
        return await GetAll(GetKillmailPage, limit, cancellationToken);
    }

    public async Task<Result<Killmail>> GetKillmailById(string id, string? format = null, CancellationToken cancellationToken = default) {
        var requestModel = new GetKillmailById {
            KillmailId = id,
            Format = format
        };
        var result = await httpClient.Get<GetKillmailById, Killmail>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<IEnumerable<WorldApiConfig>>> GetConfig(CancellationToken cancellationToken = default) {
        var requestModel = new GetConfig();
        var result = await httpClient.Get<GetConfig, IEnumerable<WorldApiConfig>>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    // Tribes
    public async Task<Result<WorldApiPayload<Tribe>>> GetTribesPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default) {
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

    public async Task<Result<Tribe>> GetTribeById(long id, string? format = null, CancellationToken cancellationToken = default) {
        var requestModel = new GetTribeById {
            TribeId = id,
            Format = format
        };
        var result = await httpClient.Get<GetTribeById, Tribe>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<VerifyPodResponse>> VerifyPod(object podData, CancellationToken cancellationToken = default) {
        var requestModel = new PostVerifyPod {
            PodData = podData
        };
        var result = await httpClient.Post<PostVerifyPod, VerifyPodResponse>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    public async Task<Result<string>> GetHealth(CancellationToken cancellationToken = default) {
        var requestModel = new GetHealth();
        var result = await httpClient.Get<GetHealth, string>(requestModel, cancellationToken);
        return result.IsFailed ? Result.Fail(result.Errors) : Result.Ok(result.Value);
    }

    private async static Task<Result<IEnumerable<T>>> GetAll<T>(Func<long, long, CancellationToken, Task<Result<WorldApiPayload<T>>>> pageFunction,
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