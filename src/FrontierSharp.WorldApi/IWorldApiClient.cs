using FluentResults;
using FrontierSharp.WorldApi.Models;

namespace FrontierSharp.WorldApi;

public interface IWorldApiClient {
    Task<Result<GameType>> GetTypeById(long id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<GameType>>> GetAllTypes(long limit = 100, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SolarSystem>>> GetAllSolarSystems(long limit = 1000, CancellationToken cancellationToken = default);
    Task<Result<SolarSystemDetail>> GetSolarSystemById(long id, CancellationToken cancellationToken = default);
    Task<Result<WorldApiPayload<GameType>>> GetTypesPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default);
    Task<Result<WorldApiPayload<SolarSystem>>> GetSolarSystemPage(long limit = 1000, long offset = 0, CancellationToken cancellationToken = default);
    Task<Result<WorldApiPayload<Tribe>>> GetTribesPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Tribe>>> GetAllTribes(long limit = 100, CancellationToken cancellationToken = default);
    Task<Result<TribeDetail>> GetTribeById(long id, CancellationToken cancellationToken = default);
}