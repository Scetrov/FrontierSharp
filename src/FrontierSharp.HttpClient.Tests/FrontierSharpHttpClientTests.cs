using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using FrontierSharp.HttpClient.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FrontierSharp.HttpClient.Tests;

// Fake Request Model extending GetRequestModel<T>
public class FakeRequest : GetRequestModel<FakeRequest> {
    public override string GetCacheKey() {
        return "FakeRequestCacheKey";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string>();
    }

    public override string GetEndpoint() {
        return "/test";
    }
}

// Fake Response Model used for deserialization
public class FakeResponse {
    public string Message { get; init; } = string.Empty;
}

internal sealed class FakeHybridCache : HybridCache {
    public override ValueTask<T> GetOrCreateAsync<TState, T>(string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default) {
        return factory(state, cancellationToken);
    }

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default) {
        return default;
    }

    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default) {
        return default;
    }

    public override ValueTask SetAsync<T>(string key, T value, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default) {
        return default;
    }
}

// A simple HttpMessageHandler to simulate HTTP responses.
public class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler {

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
        return Task.FromResult(response);
    }
}

public class FrontierSharpHttpClientTests {
    private readonly IOptions<FrontierSharpHttpClientOptions> _options;

    public FrontierSharpHttpClientTests() {
        // Setup options with a base URI and an HTTP client name.
        var options = new FrontierSharpHttpClientOptions {
            BaseUri = "http://localhost",
            HttpClientName = "TestClient"
        };
        _options = Options.Create(options);
    }

    [Fact]
    public async Task Get_ReturnsFailure_WhenResponseIsNotSuccessful() {
        // Arrange
        var loggerMock = Substitute.For<ILogger<FrontierSharpHttpClient>>();
        var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
        var cache = new FakeHybridCache();

        // Simulate an HTTP response with a non-success status code.
        const HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        const string reason = "Internal Server Error";
        var responseMessage = new HttpResponseMessage(statusCode) {
            ReasonPhrase = reason
        };

        var httpClient = new System.Net.Http.HttpClient(new FakeHttpMessageHandler(responseMessage));
        httpClientFactoryMock.CreateClient(Arg.Any<string>()).Returns(httpClient);

        var client = new FrontierSharpHttpClient(
            loggerMock,
            httpClientFactoryMock,
            cache,
            _options);

        var requestModel = new FakeRequest();

        // Act
        var result = await client.Get<FakeRequest, FakeResponse>(requestModel);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain($"Request failed with status code {(int)statusCode} ({reason}).");
    }

    [Fact]
    public async Task Get_ReturnsSuccess_WhenResponseIsSuccessful_AndDeserializationSucceeds() {
        // Arrange
        var loggerMock = Substitute.For<ILogger<FrontierSharpHttpClient>>();
        var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
        var cache = new FakeHybridCache();

        // Prepare a valid JSON response.
        var fakeResponse = new FakeResponse { Message = "Hello" };
        var json = JsonSerializer.Serialize(fakeResponse);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var httpClient = new System.Net.Http.HttpClient(new FakeHttpMessageHandler(responseMessage));
        httpClientFactoryMock.CreateClient(Arg.Any<string>()).Returns(httpClient);

        var client = new FrontierSharpHttpClient(
            loggerMock,
            httpClientFactoryMock,
            cache,
            _options);

        var requestModel = new FakeRequest();

        // Act
        var result = await client.Get<FakeRequest, FakeResponse>(requestModel);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Message.Should().Be("Hello");
    }

    [Fact]
    public async Task Get_ReturnsFailure_WhenDeserializationFails() {
        // Arrange
        var loggerMock = Substitute.For<ILogger<FrontierSharpHttpClient>>();
        var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
        var cache = new FakeHybridCache();

        // Provide a JSON that deserializes to null.
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        var httpClient = new System.Net.Http.HttpClient(new FakeHttpMessageHandler(responseMessage));
        httpClientFactoryMock.CreateClient(Arg.Any<string>()).Returns(httpClient);

        var client = new FrontierSharpHttpClient(
            loggerMock,
            httpClientFactoryMock,
            cache,
            _options);

        var requestModel = new FakeRequest();

        // Act
        var result = await client.Get<FakeRequest, FakeResponse>(requestModel);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Unable to deserialize the response into a JSON object, resulted in a null object.");
    }
}