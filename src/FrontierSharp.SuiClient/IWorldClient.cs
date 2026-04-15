using FluentResults;
using FrontierSharp.SuiClient.Models;

namespace FrontierSharp.SuiClient;

public interface IWorldClient {
    Task<Result<PagedResult<Killmail>>> GetKillmailsAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null);

    Task<Result<IEnumerable<Killmail>>> GetAllKillmailsAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null);

    Task<Result<PagedResult<Character>>> GetCharactersAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null);

    Task<Result<IEnumerable<Character>>> GetAllCharactersAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null);

    Task<Result<PagedResult<Assembly>>> GetAssembliesAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null);

    Task<Result<IEnumerable<Assembly>>> GetAllAssembliesAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null);

    Task<Result<IAssemblyUpdateSubscription>> SubscribeToAssemblyUpdatesAsync(
        Func<AssemblyUpdateBatch, CancellationToken, Task> onUpdate,
        AssemblySubscriptionOptions? options = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null);

    Task<Result<IAssemblyUpdateSubscription>> SubscribeToAssemblyUpdatesAsync(
        IEnumerable<Assembly> currentAssemblies,
        Func<AssemblyUpdateBatch, CancellationToken, Task> onUpdate,
        AssemblySubscriptionOptions? options = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null);

    Task<Result<IKillmailUpdateSubscription>> SubscribeToKillmailUpdatesAsync(
        Func<KillmailUpdateBatch, CancellationToken, Task> onUpdate,
        KillmailSubscriptionOptions? options = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null);

    Task<Result<IKillmailUpdateSubscription>> SubscribeToKillmailUpdatesAsync(
        IEnumerable<Killmail> currentKillmails,
        Func<KillmailUpdateBatch, CancellationToken, Task> onUpdate,
        KillmailSubscriptionOptions? options = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null);

    Task<Result<ICharacterUpdateSubscription>> SubscribeToCharacterUpdatesAsync(
        Func<CharacterUpdateBatch, CancellationToken, Task> onUpdate,
        CharacterSubscriptionOptions? options = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null);

    Task<Result<ICharacterUpdateSubscription>> SubscribeToCharacterUpdatesAsync(
        IEnumerable<Character> currentCharacters,
        Func<CharacterUpdateBatch, CancellationToken, Task> onUpdate,
        CharacterSubscriptionOptions? options = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null);
