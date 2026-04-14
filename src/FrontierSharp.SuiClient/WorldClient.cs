using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentResults;
using FrontierSharp.SuiClient.GraphQl;
using FrontierSharp.SuiClient.JsonConverters;
using FrontierSharp.SuiClient.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FrontierSharp.SuiClient;

public class WorldClient(
    ISuiGraphQlClient graphQlClient,
    IOptions<SuiClientOptions> options,
    ILogger<WorldClient> logger) : IWorldClient {
    private static readonly JsonSerializerOptions MoveJsonOptions = CreateMoveJsonOptions();
    private static readonly GraphQlQueryOptions NoCacheQueryOptions = new() { BypassCache = true };

    public async Task<Result<PagedResult<Killmail>>> GetKillmailsAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null) {
        var moveType = WorldQueries.KillmailType(ResolveWorldPackageAddress(worldPackageAddress));
        return await QueryObjectsAsync<Killmail>(moveType, first, after, cancellationToken);
    }

    public async Task<Result<IEnumerable<Killmail>>> GetAllKillmailsAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null) {
        var moveType = WorldQueries.KillmailType(ResolveWorldPackageAddress(worldPackageAddress));
        return await QueryAllObjectsAsync<Killmail>(moveType, first, after, cancellationToken);
    }

    public async Task<Result<PagedResult<Character>>> GetCharactersAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null) {
        var moveType = WorldQueries.CharacterType(ResolveWorldPackageAddress(worldPackageAddress));
        return await QueryObjectsAsync<Character>(moveType, first, after, cancellationToken);
    }

    public async Task<Result<IEnumerable<Character>>> GetAllCharactersAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null) {
        var moveType = WorldQueries.CharacterType(ResolveWorldPackageAddress(worldPackageAddress));
        return await QueryAllObjectsAsync<Character>(moveType, first, after, cancellationToken);
    }

    public async Task<Result<PagedResult<Assembly>>> GetAssembliesAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null) {
        var moveType = WorldQueries.AssemblyType(ResolveWorldPackageAddress(worldPackageAddress));
        return await QueryObjectsAsync<Assembly>(moveType, first, after, cancellationToken);
    }

    public async Task<Result<IEnumerable<Assembly>>> GetAllAssembliesAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null) {
        var moveType = WorldQueries.AssemblyType(ResolveWorldPackageAddress(worldPackageAddress));
        return await QueryAllObjectsAsync<Assembly>(moveType, first, after, cancellationToken);
    }

    public async Task<Result<IAssemblyUpdateSubscription>> SubscribeToAssemblyUpdatesAsync(
        Func<AssemblyUpdateBatch, CancellationToken, Task> onUpdate,
        AssemblySubscriptionOptions? subscriptionOptions = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null) {
        var effectiveOptions = subscriptionOptions ?? new AssemblySubscriptionOptions();
        var validationResult = ValidateAssemblySubscription(onUpdate, effectiveOptions);
        if (validationResult.IsFailed)
            return Result.Fail<IAssemblyUpdateSubscription>(validationResult.Errors);

        var moveType = WorldQueries.AssemblyType(ResolveWorldPackageAddress(worldPackageAddress));
        var initialAssembliesResult = await QueryAllObjectsAsync<Assembly>(
            moveType,
            effectiveOptions.PageSize,
            null,
            cancellationToken,
            NoCacheQueryOptions);

        if (initialAssembliesResult.IsFailed)
            return Result.Fail<IAssemblyUpdateSubscription>(initialAssembliesResult.Errors);

        return CreateAssemblySubscription(initialAssembliesResult.Value, moveType, onUpdate, effectiveOptions, cancellationToken);
    }

    public Task<Result<IAssemblyUpdateSubscription>> SubscribeToAssemblyUpdatesAsync(
        IEnumerable<Assembly> currentAssemblies,
        Func<AssemblyUpdateBatch, CancellationToken, Task> onUpdate,
        AssemblySubscriptionOptions? subscriptionOptions = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null) {
        var effectiveOptions = subscriptionOptions ?? new AssemblySubscriptionOptions();
        var validationResult = ValidateAssemblySubscription(onUpdate, effectiveOptions);
        if (validationResult.IsFailed)
            return Task.FromResult(Result.Fail<IAssemblyUpdateSubscription>(validationResult.Errors));

        var moveType = WorldQueries.AssemblyType(ResolveWorldPackageAddress(worldPackageAddress));
        return Task.FromResult(CreateAssemblySubscription(currentAssemblies, moveType, onUpdate, effectiveOptions, cancellationToken));
    }

    public async Task<Result<IKillmailUpdateSubscription>> SubscribeToKillmailUpdatesAsync(
        Func<KillmailUpdateBatch, CancellationToken, Task> onUpdate,
        KillmailSubscriptionOptions? subscriptionOptions = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null) {
        var effectiveOptions = subscriptionOptions ?? new KillmailSubscriptionOptions();
        var validationResult = ValidateKillmailSubscription(onUpdate, effectiveOptions);
        if (validationResult.IsFailed)
            return Result.Fail<IKillmailUpdateSubscription>(validationResult.Errors);

        var moveType = WorldQueries.KillmailType(ResolveWorldPackageAddress(worldPackageAddress));
        var initialKillmailsResult = await QueryAllObjectsAsync<Killmail>(
            moveType,
            effectiveOptions.PageSize,
            null,
            cancellationToken,
            NoCacheQueryOptions);

        if (initialKillmailsResult.IsFailed)
            return Result.Fail<IKillmailUpdateSubscription>(initialKillmailsResult.Errors);

        return CreateKillmailSubscription(initialKillmailsResult.Value, moveType, onUpdate, effectiveOptions, cancellationToken);
    }

    public Task<Result<IKillmailUpdateSubscription>> SubscribeToKillmailUpdatesAsync(
        IEnumerable<Killmail> currentKillmails,
        Func<KillmailUpdateBatch, CancellationToken, Task> onUpdate,
        KillmailSubscriptionOptions? subscriptionOptions = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null) {
        var effectiveOptions = subscriptionOptions ?? new KillmailSubscriptionOptions();
        var validationResult = ValidateKillmailSubscription(onUpdate, effectiveOptions);
        if (validationResult.IsFailed)
            return Task.FromResult(Result.Fail<IKillmailUpdateSubscription>(validationResult.Errors));

        var moveType = WorldQueries.KillmailType(ResolveWorldPackageAddress(worldPackageAddress));
        return Task.FromResult(CreateKillmailSubscription(currentKillmails, moveType, onUpdate, effectiveOptions, cancellationToken));
    }

    public async Task<Result<ICharacterUpdateSubscription>> SubscribeToCharacterUpdatesAsync(
        Func<CharacterUpdateBatch, CancellationToken, Task> onUpdate,
        CharacterSubscriptionOptions? subscriptionOptions = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null) {
        var effectiveOptions = subscriptionOptions ?? new CharacterSubscriptionOptions();
        var validationResult = ValidateCharacterSubscription(onUpdate, effectiveOptions);
        if (validationResult.IsFailed)
            return Result.Fail<ICharacterUpdateSubscription>(validationResult.Errors);

        var moveType = WorldQueries.CharacterType(ResolveWorldPackageAddress(worldPackageAddress));
        var initialCharactersResult = await QueryAllObjectsAsync<Character>(
            moveType,
            effectiveOptions.PageSize,
            null,
            cancellationToken,
            NoCacheQueryOptions);

        if (initialCharactersResult.IsFailed)
            return Result.Fail<ICharacterUpdateSubscription>(initialCharactersResult.Errors);

        return CreateCharacterSubscription(initialCharactersResult.Value, moveType, onUpdate, effectiveOptions, cancellationToken);
    }

    public Task<Result<ICharacterUpdateSubscription>> SubscribeToCharacterUpdatesAsync(
        IEnumerable<Character> currentCharacters,
        Func<CharacterUpdateBatch, CancellationToken, Task> onUpdate,
        CharacterSubscriptionOptions? subscriptionOptions = null,
        CancellationToken cancellationToken = default,
        string? worldPackageAddress = null) {
        var effectiveOptions = subscriptionOptions ?? new CharacterSubscriptionOptions();
        var validationResult = ValidateCharacterSubscription(onUpdate, effectiveOptions);
        if (validationResult.IsFailed)
            return Task.FromResult(Result.Fail<ICharacterUpdateSubscription>(validationResult.Errors));

        var moveType = WorldQueries.CharacterType(ResolveWorldPackageAddress(worldPackageAddress));
        return Task.FromResult(CreateCharacterSubscription(currentCharacters, moveType, onUpdate, effectiveOptions, cancellationToken));
    }

    private async Task<Result<PagedResult<T>>> QueryObjectsAsync<T>(
        string moveType,
        int first,
        Cursor? after,
        CancellationToken cancellationToken,
        GraphQlQueryOptions? queryOptions = null) where T : class {
        var variables = new Dictionary<string, object?> {
            ["type"] = moveType,
            ["first"] = first,
            ["after"] = after?.Value
        };

        logger.LogInformation(
            "Querying objects of type {MoveType} (first={First}, after={After})",
            moveType, first, after);

        var result = queryOptions == null
            ? await graphQlClient.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, variables, cancellationToken)
            : await graphQlClient.QueryAsync<ObjectsQueryData>(WorldQueries.GetObjectsByType, variables, queryOptions, cancellationToken);

        if (result.IsFailed) return Result.Fail<PagedResult<T>>(result.Errors);

        var connection = result.Value.Objects;
        var items = new List<T>();
        var deserializationErrors = new List<IError>();

        foreach (var node in connection.Nodes) {
            if (node.AsMoveObject?.Contents == null) {
                logger.LogWarning("Object {Address} has no Move content, skipping", node.Address);
                continue;
            }

            try {
                var item = node.AsMoveObject.Contents.Json.Deserialize<T>(MoveJsonOptions);
                if (item != null) {
                    items.Add(item);
                }
                else {
                    logger.LogWarning("Deserialization of object {Address} returned null", node.Address);
                    deserializationErrors.Add(new Error(
                        $"Deserialization of object {node.Address} returned null for type {typeof(T).Name}."));
                }
            }
            catch (JsonException ex) {
                logger.LogWarning(ex, "Failed to deserialize object {Address}", node.Address);
                deserializationErrors.Add(new Error(
                    $"Failed to deserialize object {node.Address} as {typeof(T).Name}: {ex.Message}"));
            }
        }

        if (deserializationErrors.Count > 0)
            return Result.Fail<PagedResult<T>>(deserializationErrors);

        return Result.Ok(new PagedResult<T> {
            Data = items,
            HasNextPage = connection.PageInfo.HasNextPage,
            EndCursor = connection.PageInfo.EndCursor == null ? null : new Cursor(connection.PageInfo.EndCursor)
        });
    }

    private async Task<Result<IEnumerable<T>>> QueryAllObjectsAsync<T>(
        string moveType,
        int first,
        Cursor? after,
        CancellationToken cancellationToken,
        GraphQlQueryOptions? queryOptions = null) where T : class {
        var allItems = new List<T>();
        var cursor = after;

        while (true) {
            var result = await QueryObjectsAsync<T>(moveType, first, cursor, cancellationToken, queryOptions);
            if (result.IsFailed)
                return Result.Fail<IEnumerable<T>>(result.Errors);

            allItems.AddRange(result.Value.Data);

            if (!result.Value.HasNextPage)
                return Result.Ok<IEnumerable<T>>(allItems);

            if (result.Value.EndCursor == null) {
                logger.LogWarning(
                    "Query for objects of type {MoveType} indicated more pages but did not provide an end cursor",
                    moveType);
                return Result.Fail<IEnumerable<T>>(
                    new Error($"Query for objects of type {moveType} indicated more pages but did not provide an end cursor."));
            }

            cursor = result.Value.EndCursor;
        }
    }

    private Result<IAssemblyUpdateSubscription> CreateAssemblySubscription(
        IEnumerable<Assembly> currentAssemblies,
        string moveType,
        Func<AssemblyUpdateBatch, CancellationToken, Task> onUpdate,
        AssemblySubscriptionOptions subscriptionOptions,
        CancellationToken cancellationToken) {
        var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var completion = RunAssemblySubscriptionLoopAsync(currentAssemblies.ToList(), moveType, onUpdate, subscriptionOptions,
            linkedCancellationTokenSource.Token);
        return Result.Ok<IAssemblyUpdateSubscription>(new AssemblyUpdateSubscription(linkedCancellationTokenSource, completion));
    }

    private async Task RunAssemblySubscriptionLoopAsync(
        IReadOnlyList<Assembly> initialAssemblies,
        string moveType,
        Func<AssemblyUpdateBatch, CancellationToken, Task> onUpdate,
        AssemblySubscriptionOptions subscriptionOptions,
        CancellationToken cancellationToken) {
        try {
            var previousSnapshot = CreateAssemblySnapshot(initialAssemblies);

            if (subscriptionOptions.EmitInitialSnapshot)
                await onUpdate(new AssemblyUpdateBatch {
                    IsInitialSnapshot = true,
                    CurrentAssemblies = initialAssemblies,
                    Changes = []
                }, cancellationToken);

            while (true) {
                await Task.Delay(subscriptionOptions.PollInterval, cancellationToken);

                var latestAssembliesResult = await QueryAllObjectsAsync<Assembly>(
                    moveType,
                    subscriptionOptions.PageSize,
                    null,
                    cancellationToken,
                    NoCacheQueryOptions);

                if (latestAssembliesResult.IsFailed)
                    throw CreateSubscriptionFailure("Assembly", latestAssembliesResult.Errors);

                var latestAssemblies = latestAssembliesResult.Value.ToList();
                var currentSnapshot = CreateAssemblySnapshot(latestAssemblies);
                var changes = DiffAssemblies(previousSnapshot, currentSnapshot);

                if (changes.Count > 0)
                    await onUpdate(new AssemblyUpdateBatch {
                        CurrentAssemblies = latestAssemblies,
                        Changes = changes
                    }, cancellationToken);

                previousSnapshot = currentSnapshot;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            logger.LogDebug("Assembly subscription polling canceled.");
        }
    }

    private static Result ValidateAssemblySubscription(
        Func<AssemblyUpdateBatch, CancellationToken, Task>? onUpdate,
        AssemblySubscriptionOptions options) {
        var errors = new List<IError>();

        if (onUpdate == null)
            errors.Add(new Error("Assembly update callback cannot be null."));

        if (options.PageSize <= 0)
            errors.Add(new Error("Assembly subscription page size must be greater than zero."));

        if (options.PollInterval <= TimeSpan.Zero)
            errors.Add(new Error("Assembly subscription poll interval must be greater than zero."));

        return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
    }

    private static Dictionary<string, Assembly> CreateAssemblySnapshot(IEnumerable<Assembly> assemblies) {
        var snapshot = new Dictionary<string, Assembly>(StringComparer.Ordinal);

        foreach (var assembly in assemblies)
            snapshot[CreateAssemblySnapshotKey(assembly)] = assembly;

        return snapshot;
    }

    private static List<AssemblyChange> DiffAssemblies(
        IReadOnlyDictionary<string, Assembly> previousSnapshot,
        IReadOnlyDictionary<string, Assembly> currentSnapshot) {
        var changes = new List<AssemblyChange>();

        foreach (var pair in currentSnapshot) {
            var key = pair.Key;
            var currentAssembly = pair.Value;
            if (!previousSnapshot.TryGetValue(key, out var previousAssembly)) {
                changes.Add(new AssemblyChange {
                    ChangeType = AssemblyChangeType.Added,
                    Current = currentAssembly
                });
                continue;
            }

            if (!AssembliesMatch(previousAssembly, currentAssembly))
                changes.Add(new AssemblyChange {
                    ChangeType = AssemblyChangeType.Updated,
                    Previous = previousAssembly,
                    Current = currentAssembly
                });
        }

        foreach (var pair in previousSnapshot) {
            var key = pair.Key;
            var previousAssembly = pair.Value;
            if (currentSnapshot.ContainsKey(key))
                continue;

            changes.Add(new AssemblyChange {
                ChangeType = AssemblyChangeType.Removed,
                Previous = previousAssembly
            });
        }

        return changes;
    }

    private static string CreateAssemblySnapshotKey(Assembly assembly) {
        return $"{assembly.Key.Tenant}:{assembly.Key.ItemId}";
    }

    private static bool AssembliesMatch(Assembly left, Assembly right) {
        return left.Key.ItemId == right.Key.ItemId
               && string.Equals(left.Key.Tenant, right.Key.Tenant, StringComparison.Ordinal)
               && left.TypeId == right.TypeId
               && string.Equals(left.OwnerCapId, right.OwnerCapId, StringComparison.Ordinal)
               && left.Status == right.Status
               && string.Equals(left.Location, right.Location, StringComparison.Ordinal)
               && string.Equals(left.EnergySourceId, right.EnergySourceId, StringComparison.Ordinal);
    }

    private static InvalidOperationException CreateSubscriptionFailure(string subscriptionName, IReadOnlyList<IError> errors) {
        var message = string.Join("; ", errors.Select(error => error.Message));
        return new InvalidOperationException($"{subscriptionName} subscription polling failed: {message}");
    }

    private static JsonSerializerOptions CreateMoveJsonOptions() {
        var jsonOptions = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };
        jsonOptions.Converters.Add(new SuiU64Converter());
        jsonOptions.Converters.Add(new MoveEnumConverter<LossType>());
        jsonOptions.Converters.Add(new MoveEnumConverter<AssemblyStatus>());
        return jsonOptions;
    }

    private Result<IKillmailUpdateSubscription> CreateKillmailSubscription(
        IEnumerable<Killmail> currentKillmails,
        string moveType,
        Func<KillmailUpdateBatch, CancellationToken, Task> onUpdate,
        KillmailSubscriptionOptions subscriptionOptions,
        CancellationToken cancellationToken) {
        var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var completion = RunKillmailSubscriptionLoopAsync(currentKillmails.ToList(), moveType, onUpdate, subscriptionOptions,
            linkedCancellationTokenSource.Token);
        return Result.Ok<IKillmailUpdateSubscription>(new KillmailUpdateSubscription(linkedCancellationTokenSource, completion));
    }

    private async Task RunKillmailSubscriptionLoopAsync(
        IReadOnlyList<Killmail> initialKillmails,
        string moveType,
        Func<KillmailUpdateBatch, CancellationToken, Task> onUpdate,
        KillmailSubscriptionOptions subscriptionOptions,
        CancellationToken cancellationToken) {
        try {
            var previousSnapshot = CreateKillmailSnapshot(initialKillmails);

            if (subscriptionOptions.EmitInitialSnapshot)
                await onUpdate(new KillmailUpdateBatch {
                    IsInitialSnapshot = true,
                    CurrentKillmails = initialKillmails,
                    Changes = []
                }, cancellationToken);

            while (true) {
                await Task.Delay(subscriptionOptions.PollInterval, cancellationToken);

                var latestKillmailsResult = await QueryAllObjectsAsync<Killmail>(
                    moveType,
                    subscriptionOptions.PageSize,
                    null,
                    cancellationToken,
                    NoCacheQueryOptions);

                if (latestKillmailsResult.IsFailed)
                    throw CreateSubscriptionFailure("Killmail", latestKillmailsResult.Errors);

                var latestKillmails = latestKillmailsResult.Value.ToList();
                var currentSnapshot = CreateKillmailSnapshot(latestKillmails);
                var changes = DiffKillmails(previousSnapshot, currentSnapshot);

                if (changes.Count > 0)
                    await onUpdate(new KillmailUpdateBatch {
                        CurrentKillmails = latestKillmails,
                        Changes = changes
                    }, cancellationToken);

                previousSnapshot = currentSnapshot;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            logger.LogDebug("Killmail subscription polling canceled.");
        }
    }

    private static Result ValidateKillmailSubscription(
        Func<KillmailUpdateBatch, CancellationToken, Task>? onUpdate,
        KillmailSubscriptionOptions options) {
        var errors = new List<IError>();

        if (onUpdate == null)
            errors.Add(new Error("Killmail update callback cannot be null."));

        if (options.PageSize <= 0)
            errors.Add(new Error("Killmail subscription page size must be greater than zero."));

        if (options.PollInterval <= TimeSpan.Zero)
            errors.Add(new Error("Killmail subscription poll interval must be greater than zero."));

        return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
    }

    private static Dictionary<string, Killmail> CreateKillmailSnapshot(IEnumerable<Killmail> killmails) {
        var snapshot = new Dictionary<string, Killmail>(StringComparer.Ordinal);

        foreach (var killmail in killmails)
            snapshot[killmail.Id] = killmail;

        return snapshot;
    }

    private static List<KillmailChange> DiffKillmails(
        IReadOnlyDictionary<string, Killmail> previousSnapshot,
        IReadOnlyDictionary<string, Killmail> currentSnapshot) {
        var changes = new List<KillmailChange>();

        foreach (var pair in currentSnapshot) {
            var id = pair.Key;
            var currentKillmail = pair.Value;
            if (!previousSnapshot.TryGetValue(id, out var previousKillmail)) {
                changes.Add(new KillmailChange {
                    ChangeType = KillmailChangeType.Added,
                    Current = currentKillmail
                });
                continue;
            }

            if (!KillmailsMatch(previousKillmail, currentKillmail))
                changes.Add(new KillmailChange {
                    ChangeType = KillmailChangeType.Updated,
                    Previous = previousKillmail,
                    Current = currentKillmail
                });
        }

        foreach (var pair in previousSnapshot) {
            var id = pair.Key;
            var previousKillmail = pair.Value;
            if (currentSnapshot.ContainsKey(id))
                continue;

            changes.Add(new KillmailChange {
                ChangeType = KillmailChangeType.Removed,
                Previous = previousKillmail
            });
        }

        return changes;
    }

    private static bool KillmailsMatch(Killmail left, Killmail right) {
        return string.Equals(left.Id, right.Id, StringComparison.Ordinal)
               && left.KillerId == right.KillerId
               && left.VictimId == right.VictimId
               && left.ReportedByCharacterId == right.ReportedByCharacterId
               && left.KillTimestamp == right.KillTimestamp
               && left.LossType == right.LossType
               && left.SolarSystemId == right.SolarSystemId;
    }

    private Result<ICharacterUpdateSubscription> CreateCharacterSubscription(
        IEnumerable<Character> currentCharacters,
        string moveType,
        Func<CharacterUpdateBatch, CancellationToken, Task> onUpdate,
        CharacterSubscriptionOptions subscriptionOptions,
        CancellationToken cancellationToken) {
        var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var completion = RunCharacterSubscriptionLoopAsync(currentCharacters.ToList(), moveType, onUpdate, subscriptionOptions,
            linkedCancellationTokenSource.Token);
        return Result.Ok<ICharacterUpdateSubscription>(new CharacterUpdateSubscription(linkedCancellationTokenSource, completion));
    }

    private async Task RunCharacterSubscriptionLoopAsync(
        IReadOnlyList<Character> initialCharacters,
        string moveType,
        Func<CharacterUpdateBatch, CancellationToken, Task> onUpdate,
        CharacterSubscriptionOptions subscriptionOptions,
        CancellationToken cancellationToken) {
        try {
            var previousSnapshot = CreateCharacterSnapshot(initialCharacters);

            if (subscriptionOptions.EmitInitialSnapshot)
                await onUpdate(new CharacterUpdateBatch {
                    IsInitialSnapshot = true,
                    CurrentCharacters = initialCharacters,
                    Changes = []
                }, cancellationToken);

            while (true) {
                await Task.Delay(subscriptionOptions.PollInterval, cancellationToken);

                var latestCharactersResult = await QueryAllObjectsAsync<Character>(
                    moveType,
                    subscriptionOptions.PageSize,
                    null,
                    cancellationToken,
                    NoCacheQueryOptions);

                if (latestCharactersResult.IsFailed)
                    throw CreateSubscriptionFailure("Character", latestCharactersResult.Errors);

                var latestCharacters = latestCharactersResult.Value.ToList();
                var currentSnapshot = CreateCharacterSnapshot(latestCharacters);
                var changes = DiffCharacters(previousSnapshot, currentSnapshot);

                if (changes.Count > 0)
                    await onUpdate(new CharacterUpdateBatch {
                        CurrentCharacters = latestCharacters,
                        Changes = changes
                    }, cancellationToken);

                previousSnapshot = currentSnapshot;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            logger.LogDebug("Character subscription polling canceled.");
        }
    }

    private static Result ValidateCharacterSubscription(
        Func<CharacterUpdateBatch, CancellationToken, Task>? onUpdate,
        CharacterSubscriptionOptions options) {
        var errors = new List<IError>();

        if (onUpdate == null)
            errors.Add(new Error("Character update callback cannot be null."));

        if (options.PageSize <= 0)
            errors.Add(new Error("Character subscription page size must be greater than zero."));

        if (options.PollInterval <= TimeSpan.Zero)
            errors.Add(new Error("Character subscription poll interval must be greater than zero."));

        return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
    }

    private static Dictionary<string, Character> CreateCharacterSnapshot(IEnumerable<Character> characters) {
        var snapshot = new Dictionary<string, Character>(StringComparer.Ordinal);

        foreach (var character in characters)
            snapshot[CreateCharacterSnapshotKey(character)] = character;

        return snapshot;
    }

    private static List<CharacterChange> DiffCharacters(
        IReadOnlyDictionary<string, Character> previousSnapshot,
        IReadOnlyDictionary<string, Character> currentSnapshot) {
        var changes = new List<CharacterChange>();

        foreach (var pair in currentSnapshot) {
            var key = pair.Key;
            var currentCharacter = pair.Value;
            if (!previousSnapshot.TryGetValue(key, out var previousCharacter)) {
                changes.Add(new CharacterChange {
                    ChangeType = CharacterChangeType.Added,
                    Current = currentCharacter
                });
                continue;
            }

            if (!CharactersMatch(previousCharacter, currentCharacter))
                changes.Add(new CharacterChange {
                    ChangeType = CharacterChangeType.Updated,
                    Previous = previousCharacter,
                    Current = currentCharacter
                });
        }

        foreach (var pair in previousSnapshot) {
            var key = pair.Key;
            var previousCharacter = pair.Value;
            if (currentSnapshot.ContainsKey(key))
                continue;

            changes.Add(new CharacterChange {
                ChangeType = CharacterChangeType.Removed,
                Previous = previousCharacter
            });
        }

        return changes;
    }

    private static string CreateCharacterSnapshotKey(Character character) {
        return $"{character.Key.Tenant}:{character.Key.ItemId}";
    }

    private static bool CharactersMatch(Character left, Character right) {
        return left.Key.ItemId == right.Key.ItemId
               && string.Equals(left.Key.Tenant, right.Key.Tenant, StringComparison.Ordinal)
               && left.TribeId == right.TribeId
               && string.Equals(left.CharacterAddress, right.CharacterAddress, StringComparison.Ordinal)
               && string.Equals(left.OwnerCapId, right.OwnerCapId, StringComparison.Ordinal)
               && CharacterMetadataEquals(left.Metadata, right.Metadata);
    }

    private static bool CharacterMetadataEquals(CharacterMetadata? left, CharacterMetadata? right) {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        return string.Equals(left.Name, right.Name, StringComparison.Ordinal)
               && string.Equals(left.Description, right.Description, StringComparison.Ordinal)
               && string.Equals(left.Url, right.Url, StringComparison.Ordinal)
               && string.Equals(left.AssemblyId, right.AssemblyId, StringComparison.Ordinal)
               && string.Equals(left.RawValue, right.RawValue, StringComparison.Ordinal)
               && JsonSerializer.Serialize(left.AdditionalProperties) == JsonSerializer.Serialize(right.AdditionalProperties);
    }

    private string ResolveWorldPackageAddress(string? worldPackageAddress) {
        return (string.IsNullOrWhiteSpace(worldPackageAddress)
            ? options.Value.DefaultWorldPackageAddress
            : worldPackageAddress) ?? options.Value.DefaultWorldPackageAddress;
    }
}