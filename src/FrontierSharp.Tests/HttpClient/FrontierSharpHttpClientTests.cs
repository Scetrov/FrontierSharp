﻿using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;
using FrontierSharp.Tests.Utils.FakeHttpClientFactory;
using FrontierSharp.Tests.Utils.FakeHybridCache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.HttpClient;

public class FakeRequest : GetRequestModel<FakeRequest> {
    public override string GetCacheKey() => "FakeRequestCacheKey";
    public override Dictionary<string, string> GetQueryParams() => new();
    public override string GetEndpoint() => "/test";
}

public class FakeComplexRequest : GetRequestModel<FakeComplexRequest> {
    public override string GetCacheKey() => "FakeRequestCacheKey";

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" }
        };
    }

    public override string GetEndpoint() => "/complex";
}

// Fake Response Model used for deserialization
public class FakeResponse {
    public required string Message { get; init; }
}

public class FrontierSharpHttpClientTests {
    private readonly IOptions<FrontierSharpHttpClientOptions> _options;

    public FrontierSharpHttpClientTests() {
        var options = new FrontierSharpHttpClientOptions {
            BaseUri = "http://localhost",
            HttpClientName = "TestClient"
        };
        _options = Options.Create(options);
    }

    [Fact]
    public async Task Get_ReturnsFailure_WhenResponseIsNotSuccessful() {
        var client = new FrontierSharpHttpClient(
            Substitute.For<ILogger<FrontierSharpHttpClient>>(),
            MockHttpClient.CreateSimpleSubstitute(HttpStatusCode.InternalServerError),
            new FakeHybridCache(),
            _options);

        var requestModel = new FakeRequest();

        // Act
        var result = await client.Get<FakeRequest, FakeResponse>(requestModel);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain($"Request failed with status code 500 (Internal server error).");
    }

    [Fact]
    public async Task Get_ReturnsSuccess_WhenResponseIsSuccessful_AndDeserializationSucceeds_Complex() {
        // Arrange
        var client = new FrontierSharpHttpClient(
            Substitute.For<ILogger<FrontierSharpHttpClient>>(),
            MockHttpClient.CreateSimpleSubstitute(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(new FakeResponse { Message = "Hello" }), Encoding.UTF8, "application/json")),
            new FakeHybridCache(),
            _options);

        var requestModel = new FakeComplexRequest();

        // Act
        var result = await client.Get<FakeComplexRequest, FakeResponse>(requestModel);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Message.Should().Be("Hello");
    }
    
    [Fact]
    public async Task Get_ReturnsSuccess_WhenResponseIsSuccessful_AndDeserializationSucceeds() {
        // Arrange
        var client = new FrontierSharpHttpClient(
            Substitute.For<ILogger<FrontierSharpHttpClient>>(),
            MockHttpClient.CreateSimpleSubstitute(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(new FakeResponse { Message = "Hello" }), Encoding.UTF8, "application/json")),
            new FakeHybridCache(),
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
        var client = new FrontierSharpHttpClient(
            Substitute.For<ILogger<FrontierSharpHttpClient>>(),
            MockHttpClient.CreateSimpleSubstitute(HttpStatusCode.OK, new StringContent("null", Encoding.UTF8, "application/json")),
            new FakeHybridCache(),
            _options);

        var requestModel = new FakeRequest();

        // Act
        var result = await client.Get<FakeRequest, FakeResponse>(requestModel);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Unable to deserialize the response into a JSON object, resulted in a null object.");
    }
}