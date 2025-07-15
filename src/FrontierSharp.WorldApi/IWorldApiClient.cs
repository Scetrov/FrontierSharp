using System.Numerics;
using FluentResults;
using FrontierSharp.WorldApi.Models;

namespace FrontierSharp.WorldApi;

public interface IWorldApiClient {
    Task<Result<GameType>> GetTypeById(long id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Fuel>>> GetAllFuels(long limit = 100, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<GameType>>> GetAllTypes(long limit = 100, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SmartAssemblyWithSolarSystem>>> GetAllSmartAssemblies(long limit = 100, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SmartCharacter>>> GetAllSmartCharacters(long limit = 100, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SolarSystem>>> GetAllSolarSystems(long limit = 1000, CancellationToken cancellationToken = default);
    Task<Result<SmartAssemblyDetail>> GetSmartAssemblyById(BigInteger id, CancellationToken cancellationToken = default);
    Task<Result<SmartCharacterDetail>> GetSmartCharacterById(string address, CancellationToken cancellationToken = default);
    Task<Result<SolarSystemDetail>> GetSolarSystemById(long id, CancellationToken cancellationToken = default);
    Task<Result<WorldApiPayload<Fuel>>> GetFuelsPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default);
    Task<Result<WorldApiPayload<GameType>>> GetTypesPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default);

    Task<Result<WorldApiPayload<SmartAssemblyWithSolarSystem>>> GetSmartAssemblyPage(long limit = 100, long offset = 0,
        CancellationToken cancellationToken = default);

    Task<Result<WorldApiPayload<SmartCharacter>>> GetSmartCharacterPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default);
    Task<Result<WorldApiPayload<SolarSystem>>> GetSolarSystemPage(long limit = 1000, long offset = 0, CancellationToken cancellationToken = default);
    Task<Result<WorldApiPayload<Killmail>>> GetKillmailPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Killmail>>> GetAllKillmails(long limit = 100, CancellationToken cancellationToken = default);
}