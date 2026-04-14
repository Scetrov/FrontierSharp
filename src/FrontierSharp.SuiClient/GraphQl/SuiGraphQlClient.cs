using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentResults;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FrontierSharp.SuiClient.GraphQl;

public class SuiGraphQlClient(
    IHttpClientFactory httpClientFactory,
    HybridCache cache,
    IOptions<SuiClientOptions> options,
    ILogger<SuiGraphQlClient> logger) : ISuiGraphQlClient {

    private static readonly JsonSerializerOptions SerializerOptions = new() {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    private readonly HybridCacheEntryOptions _cacheOptions = new() {
        Expiration = options.Value.GraphQlCacheDuration
    };

    public Task<Result<T>> QueryAsync<T>(string query, Dictionary<string, object?>? variables = null,
        CancellationToken cancellationToken = default) where T : class {
        return QueryAsync<T>(query, variables, queryOptions: null, cancellationToken);
    }

    public async Task<Result<T>> QueryAsync<T>(string query, Dictionary<string, object?>? variables,
        GraphQlQueryOptions? queryOptions,
        CancellationToken cancellationToken = default) where T : class {
        queryOptions ??= new GraphQlQueryOptions();

        if (queryOptions.BypassCache)
            return await ExecuteQueryAsync<T>(query, variables, cancellationToken);

        var cacheKey = CreateCacheKey<T>(query, variables);

        try {
            var data = await cache.GetOrCreateAsync(cacheKey, async ct => {
                var result = await ExecuteQueryAsync<T>(query, variables, ct);
                if (result.IsFailed)
                    throw new GraphQlQueryFailedException(result.Errors);

                return result.Value;
            }, options: _cacheOptions, cancellationToken: cancellationToken);

            return Result.Ok(data);
        } catch (GraphQlQueryFailedException ex) {
            return Result.Fail<T>(ex.Errors);
        }
    }

    private async Task<Result<T>> ExecuteQueryAsync<T>(string query, Dictionary<string, object?>? variables,
        CancellationToken cancellationToken) where T : class {
        var client = httpClientFactory.CreateClient(options.Value.HttpClientName);

        var request = new GraphQlRequest {
            Query = query,
            Variables = variables
        };

        var requestJson = JsonSerializer.Serialize(request, SerializerOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        logger.LogDebug("Sending GraphQL query to {Endpoint} with payload {Payload}", options.Value.GraphQlEndpoint,
            requestJson);

        HttpResponseMessage response;
        try {
            response = await client.PostAsync(options.Value.GraphQlEndpoint, content, cancellationToken);
        } catch (Exception ex) {
            logger.LogError(ex, "GraphQL request failed with exception");
            return Result.Fail<T>($"GraphQL request failed: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode) {
            var errorBody = await response.Content.ReadAsStringAsync();
            logger.LogError("GraphQL request failed with status {StatusCode}: {Body}", response.StatusCode, errorBody);
            return Result.Fail<T>(
                $"GraphQL request failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}).");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        logger.LogDebug("Received GraphQL response from {Endpoint} with payload {Payload}", options.Value.GraphQlEndpoint,
            responseJson);

        GraphQlResponse<T>? graphQlResponse;
        try {
            graphQlResponse = JsonSerializer.Deserialize<GraphQlResponse<T>>(responseJson, SerializerOptions);
        } catch (Exception ex) {
            logger.LogError(ex, "Failed to deserialize GraphQL response: {Body}", responseJson);
            return Result.Fail<T>($"Failed to deserialize GraphQL response: {ex.Message}");
        }

        if (graphQlResponse == null) {
            logger.LogError("GraphQL response deserialized to null: {Body}", responseJson);
            return Result.Fail<T>("GraphQL response deserialized to null.");
        }

        if (graphQlResponse.Errors is { Count: > 0 }) {
            var errorMessages = string.Join("; ", graphQlResponse.Errors.Select(e => e.Message));
            logger.LogError("GraphQL errors: {Errors}", errorMessages);
            return Result.Fail<T>($"GraphQL errors: {errorMessages}");
        }

        if (graphQlResponse.Data == null) {
            logger.LogError("GraphQL response contained no data: {Body}", responseJson);
            return Result.Fail<T>("GraphQL response contained no data.");
        }

        return Result.Ok(graphQlResponse.Data);
    }

    private string CreateCacheKey<T>(string query, Dictionary<string, object?>? variables) {
        var normalizedVariables = variables == null ? string.Empty : NormalizeDictionary(variables);
        var cacheMaterial = $"{options.Value.GraphQlEndpoint}\n{query}\n{normalizedVariables}";
        byte[] hashBytes;
        using (var sha256 = SHA256.Create()) {
            hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(cacheMaterial));
        }
        var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        return $"SuiGraphQl_{typeof(T).FullName}_{hash}";
    }

    private static string NormalizeDictionary(IReadOnlyDictionary<string, object?> dictionary) {
        var builder = new StringBuilder();
        builder.Append('{');
        var first = true;
        foreach (var pair in dictionary.OrderBy(kvp => kvp.Key, StringComparer.Ordinal)) {
            if (!first)
                builder.Append(',');
            first = false;
            builder.Append(JsonSerializer.Serialize(pair.Key, SerializerOptions));
            builder.Append(':');
            AppendNormalizedValue(builder, pair.Value);
        }
        builder.Append('}');
        return builder.ToString();
    }

    private static void AppendNormalizedValue(StringBuilder builder, object? value) {
        switch (value) {
            case null:
                builder.Append("null");
                return;
            case JsonElement jsonElement:
                builder.Append(jsonElement.GetRawText());
                return;
            case IReadOnlyDictionary<string, object?> readOnlyDictionary:
                builder.Append(NormalizeDictionary(readOnlyDictionary));
                return;
            case IDictionary<string, object?> dictionary:
                builder.Append(NormalizeDictionary(dictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
                return;
            case IEnumerable<object?> enumerable when value is not string:
                builder.Append('[');
                var first = true;
                foreach (var item in enumerable) {
                    if (!first)
                        builder.Append(',');
                    first = false;
                    AppendNormalizedValue(builder, item);
                }
                builder.Append(']');
                return;
            default:
                builder.Append(JsonSerializer.Serialize(value, SerializerOptions));
                return;
        }
    }

    private sealed class GraphQlQueryFailedException(IReadOnlyList<IError> errors) : Exception {
        public IReadOnlyList<IError> Errors { get; } = errors;
    }
}

