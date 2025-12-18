using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentResults;
using FrontierSharp.HttpClient.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FrontierSharp.HttpClient;

public class FrontierSharpHttpClient(
    ILogger<FrontierSharpHttpClient> logger,
    IHttpClientFactory httpClientFactory,
    HybridCache cache,
    IOptions<FrontierSharpHttpClientOptions> options) : IFrontierSharpHttpClient {
    public async Task<IResult<TResponseModel>> Get<TRequestModel, TResponseModel>(TRequestModel requestModel,
        CancellationToken cancellationToken = default) where TRequestModel : GetRequestModel<TRequestModel>, new()
        where TResponseModel : class {
        return await cache.GetOrCreateAsync(requestModel.GetCacheKey(), async ct => {
            var client = httpClientFactory.CreateClient(options.Value.HttpClientName);
            var url = FormatUrl(requestModel);
            logger.LogInformation("HTTP GET {url}", url);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode) {
                logger.LogError("Request failed with status code {code} ({reason}).", response.StatusCode,
                    response.ReasonPhrase);
                return Result.Fail<TResponseModel>(
                    $"Request failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            var content = await response.Content.ReadAsStreamAsync();

            TResponseModel? result = null;
            Exception? exception = null;

            try {
                // Use configured JsonSerializerOptions if provided, otherwise ensure
                // reflection-based serialization is available by providing a
                // DefaultJsonTypeInfoResolver. This makes the client work in
                // trimmed / AOT applications that disable reflection-based
                // serialization by default.
                var serializerOptions = options.Value.JsonSerializerOptions;

                if (serializerOptions == null) {
                    serializerOptions = new JsonSerializerOptions {
                        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                    };
                } else if (serializerOptions.TypeInfoResolver == null) {
                    // Clone and set a resolver so we don't mutate the caller's instance
                    var cloned = new JsonSerializerOptions(serializerOptions);
                    cloned.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                    serializerOptions = cloned;
                }

                result = JsonSerializer.Deserialize<TResponseModel>(content, serializerOptions);
            } catch (Exception ex) {
                exception = ex;
            }

            if (result != null) return Result.Ok(result);

            content.Seek(0, SeekOrigin.Begin);
            var errorContent = await new StreamReader(content).ReadToEndAsync();
            logger.LogError("Unable to deserialize the response into a JSON object with '{exception}': {errorContent}",
                exception?.Message ?? "No Exception", errorContent);

            return Result.Fail<TResponseModel>(
                $"Unable to deserialize the response into a JSON object, resulted in a null object: {exception?.Message}");
        }, cancellationToken: cancellationToken);
    }

    private string FormatUrl<TRequestModel>(TRequestModel requestModel)
        where TRequestModel : GetRequestModel<TRequestModel>, new() {
        var builder = new UriBuilder(options.Value.BaseUri) {
            Path = requestModel.GetEndpoint(),
            Query = FormatQueryString(requestModel.GetQueryParams())
        };

        return builder.ToString();
    }

    private static string FormatQueryString(Dictionary<string, string> parameters) {
        if (parameters.Count == 0)
            return string.Empty;

        var queryString = string.Join("&",
            parameters.Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return queryString;
    }
}