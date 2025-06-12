using FluentResults;
using FrontierSharp.WorldApi.Models;

namespace FrontierSharp.WorldApi;

public interface IWorldApiClient {
    Task<Result<WorldApiPayload<GameType>>> GetTypesPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default);
    Task<Result<GameType>> GetTypeById(long id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<GameType>>> GetAllTypes(long limit = 100, CancellationToken cancellationToken = default);
    Task<Result<WorldApiPayload<Fuel>>> GetFuelsPage(long limit = 100, long offset = 0, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Fuel>>> GetAllFuels(long limit = 100, CancellationToken cancellationToken = default);
}