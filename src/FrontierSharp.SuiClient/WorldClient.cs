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
        CancellationToken cancellationToken = default) {
        var moveType = WorldQueries.KillmailType(options.Value.WorldPackageAddress);
        return await QueryObjectsAsync<Killmail>(moveType, first, after, cancellationToken);
    }

    public async Task<Result<IEnumerable<Killmail>>> GetAllKillmailsAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default) {
        var moveType = WorldQueries.KillmailType(options.Value.WorldPackageAddress);
        return await QueryAllObjectsAsync<Killmail>(moveType, first, after, cancellationToken);
    }

    public async Task<Result<PagedResult<Character>>> GetCharactersAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default) {
        var moveType = WorldQueries.CharacterType(options.Value.WorldPackageAddress);
        return await QueryObjectsAsync<Character>(moveType, first, after, cancellationToken);
    }

    public async Task<Result<IEnumerable<Character>>> GetAllCharactersAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default) {
        var moveType = WorldQueries.CharacterType(options.Value.WorldPackageAddress);
        return await QueryAllObjectsAsync<Character>(moveType, first, after, cancellationToken);
    }

    public async Task<Result<PagedResult<Assembly>>> GetAssembliesAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default) {
        var moveType = WorldQueries.AssemblyType(options.Value.WorldPackageAddress);
        return await QueryObjectsAsync<Assembly>(moveType, first, after, cancellationToken);
    }

    public async Task<Result<IEnumerable<Assembly>>> GetAllAssembliesAsync(
        int first = 50,
        Cursor? after = null,
        CancellationToken cancellationToken = default) {
        var moveType = WorldQueries.AssemblyType(options.Value.WorldPackageAddress);
        return await QueryAllObjectsAsync<Assembly>(moveType, first, after, cancellationToken);
    }

    public async Task<Result<IAssemblyUpdateSubscription>> SubscribeToAssemblyUpdatesAsync(
        Func<AssemblyUpdateBatch, CancellationToken, Task> onUpdate,
        AssemblySubscriptionOptions? subscriptionOptions = null,
        CancellationToken cancellationToken = default) {
        var effectiveOptions = subscriptionOptions ?? new AssemblySubscriptionOptions();
        var validationResult = ValidateAssemblySubscription(onUpdate, effectiveOptions);
        if (validationResult.IsFailed)
            return Result.Fail<IAssemblyUpdateSubscription>(validationResult.Errors);

        var moveType = WorldQueries.AssemblyType(options.Value.WorldPackageAddress);
        var initialAssembliesResult = await QueryAllObjectsAsync<Assembly>(
            moveType,
            effectiveOptions.PageSize,
            after: null,
            cancellationToken,
            NoCacheQueryOptions);

        if (initialAssembliesResult.IsFailed)
            return Result.Fail<IAssemblyUpdateSubscription>(initialAssembliesResult.Errors);

        return CreateAssemblySubscription(initialAssembliesResult.Value, onUpdate, effectiveOptions, cancellationToken);
    }

    public Task<Result<IAssemblyUpdateSubscription>> SubscribeToAssemblyUpdatesAsync(
        IEnumerable<Assembly> currentAssemblies,
        Func<AssemblyUpdateBatch, CancellationToken, Task> onUpdate,
        AssemblySubscriptionOptions? subscriptionOptions = null,
        CancellationToken cancellationToken = default) {
        var effectiveOptions = subscriptionOptions ?? new AssemblySubscriptionOptions();
        var validationResult = ValidateAssemblySubscription(onUpdate, effectiveOptions);
        if (validationResult.IsFailed)
            return Task.FromResult(Result.Fail<IAssemblyUpdateSubscription>(validationResult.Errors));

        return Task.FromResult(CreateAssemblySubscription(currentAssemblies, onUpdate, effectiveOptions, cancellationToken));
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

        logger.LogDebug(
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
                if (item != null)
                    items.Add(item);
                else {
                    logger.LogWarning("Deserialization of object {Address} returned null", node.Address);
                    deserializationErrors.Add(new Error(
                        $"Deserialization of object {node.Address} returned null for type {typeof(T).Name}."));
                }
            } catch (JsonException ex) {
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
        Func<AssemblyUpdateBatch, CancellationToken, Task> onUpdate,
        AssemblySubscriptionOptions subscriptionOptions,
        CancellationToken cancellationToken) {
        var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var completion = RunAssemblySubscriptionLoopAsync(currentAssemblies.ToList(), onUpdate, subscriptionOptions, linkedCancellationTokenSource.Token);
        return Result.Ok<IAssemblyUpdateSubscription>(new AssemblyUpdateSubscription(linkedCancellationTokenSource, completion));
    }

    private async Task RunAssemblySubscriptionLoopAsync(
        IReadOnlyList<Assembly> initialAssemblies,
        Func<AssemblyUpdateBatch, CancellationToken, Task> onUpdate,
        AssemblySubscriptionOptions subscriptionOptions,
        CancellationToken cancellationToken) {
        try {
            var previousSnapshot = CreateAssemblySnapshot(initialAssemblies);

            if (subscriptionOptions.EmitInitialSnapshot) {
                await onUpdate(new AssemblyUpdateBatch {
                    IsInitialSnapshot = true,
                    CurrentAssemblies = initialAssemblies,
                    Changes = []
                }, cancellationToken);
            }

            var moveType = WorldQueries.AssemblyType(options.Value.WorldPackageAddress);

            while (true) {
                await Task.Delay(subscriptionOptions.PollInterval, cancellationToken);

                var latestAssembliesResult = await QueryAllObjectsAsync<Assembly>(
                    moveType,
                    subscriptionOptions.PageSize,
                    after: null,
                    cancellationToken,
                    NoCacheQueryOptions);

                if (latestAssembliesResult.IsFailed)
                    throw CreateSubscriptionFailure(latestAssembliesResult.Errors);

                var latestAssemblies = latestAssembliesResult.Value.ToList();
                var currentSnapshot = CreateAssemblySnapshot(latestAssemblies);
                var changes = DiffAssemblies(previousSnapshot, currentSnapshot);

                if (changes.Count > 0) {
                    await onUpdate(new AssemblyUpdateBatch {
                        CurrentAssemblies = latestAssemblies,
                        Changes = changes
                    }, cancellationToken);
                }

                previousSnapshot = currentSnapshot;
            }
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
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

            if (!AssembliesMatch(previousAssembly, currentAssembly)) {
                changes.Add(new AssemblyChange {
                    ChangeType = AssemblyChangeType.Updated,
                    Previous = previousAssembly,
                    Current = currentAssembly
                });
            }
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

    private static InvalidOperationException CreateSubscriptionFailure(IReadOnlyList<IError> errors) {
        var message = string.Join("; ", errors.Select(error => error.Message));
        return new InvalidOperationException($"Assembly subscription polling failed: {message}");
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
}

