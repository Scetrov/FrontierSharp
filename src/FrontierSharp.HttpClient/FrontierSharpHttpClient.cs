using System.Text.Json;
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
        return await cache.GetOrCreateAsync<IResult<TResponseModel>>(requestModel.GetCacheKey(), async ct => {
            var url = FormatUrl(requestModel);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            return await SendAsyncInternals<TResponseModel>(url, request, ct);
        }, cancellationToken: cancellationToken);
    }
    
    public async Task<IResult<TResponseModel>> Post<TRequestModel, TResponseModel>(TRequestModel requestModel,
        CancellationToken cancellationToken = default) where TRequestModel : PostRequestModel<TRequestModel>, new()
        where TResponseModel : class {
        return await cache.GetOrCreateAsync<IResult<TResponseModel>>(requestModel.GetCacheKey(), async ct => {
            var url = FormatUrl(requestModel);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            return await SendAsyncInternals<TResponseModel>(url, request, ct);
        }, cancellationToken: cancellationToken);
    }

    private async Task<IResult<TResponseModel>> SendAsyncInternals<TResponseModel>(string url, HttpRequestMessage request, CancellationToken ct) where TResponseModel : class {
        var client = httpClientFactory.CreateClient(options.Value.HttpClientName);

        logger.LogInformation("HTTP GET {url}", url);

        var response = await client.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode) {
            logger.LogError("Request failed with status code {code} ({reason}).", response.StatusCode,
                response.ReasonPhrase);
            return Result.Fail<TResponseModel>(
                $"Request failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}).");
        }

        var content = await response.Content.ReadAsStreamAsync(ct);

        TResponseModel? result = null;
        Exception? exception = null;

        try {
            result = JsonSerializer.Deserialize<TResponseModel>(content);
        } catch (Exception ex) {
            exception = ex;
        }

        if (result != null) return Result.Ok(result);

        content.Seek(0, SeekOrigin.Begin);
        var errorContent = await new StreamReader(content).ReadToEndAsync(ct);
        logger.LogError("Unable to deserialize the response into a JSON object with '{exception}': {errorContent}",
            exception?.Message ?? "No Exception", errorContent);

        return Result.Fail<TResponseModel>(
            "Unable to deserialize the response into a JSON object, resulted in a null object.");
    }

    private string FormatUrl<TRequestModel>(TRequestModel requestModel)
        where TRequestModel : RequestModelBase, new() {
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