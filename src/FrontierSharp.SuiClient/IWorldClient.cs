using FluentResults;
using FrontierSharp.SuiClient.Models;

namespace FrontierSharp.SuiClient;

public interface IWorldClient {
    Task<Result<PagedResult<Killmail>>> GetKillmailsAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<Killmail>>> GetAllKillmailsAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<Character>>> GetCharactersAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<Character>>> GetAllCharactersAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<Assembly>>> GetAssembliesAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<Assembly>>> GetAllAssembliesAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default);
}

