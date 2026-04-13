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

    private async Task<Result<PagedResult<T>>> QueryObjectsAsync<T>(
        string moveType, int first, Cursor? after, CancellationToken cancellationToken) where T : class {
        var variables = new Dictionary<string, object?> {
            ["type"] = moveType,
            ["first"] = first,
            ["after"] = after?.Value
        };

        logger.LogDebug(
            "Querying objects of type {MoveType} (first={First}, after={After})",
            moveType, first, after);

        var result = await graphQlClient.QueryAsync<ObjectsQueryData>(
            WorldQueries.GetObjectsByType, variables, cancellationToken);

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
        string moveType, int first, Cursor? after, CancellationToken cancellationToken) where T : class {
        var allItems = new List<T>();
        var cursor = after;

        while (true) {
            var result = await QueryObjectsAsync<T>(moveType, first, cursor, cancellationToken);
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

