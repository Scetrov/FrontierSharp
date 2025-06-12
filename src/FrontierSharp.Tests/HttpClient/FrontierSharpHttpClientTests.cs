using System.Net;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using FrontierSharp.HttpClient;
using FrontierSharp.Tests.Utils.FakeHttpClientFactory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.HttpClient;

// Fake Response Model used for deserialization

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
        result.Errors[0].Message.Should().Contain("Request failed with status code 500 (Internal server error).");
    }

    [Fact]
    public async Task Get_ReturnsSuccess_WhenResponseIsSuccessful_AndDeserializationSucceeds_Complex() {
        // Arrange
        var client = new FrontierSharpHttpClient(
            Substitute.For<ILogger<FrontierSharpHttpClient>>(),
            MockHttpClient.CreateSimpleSubstitute(HttpStatusCode.OK,
                new StringContent(JsonSerializer.Serialize(new FakeResponse {
                        Message = "Hello"
                    }), Encoding.UTF8,
                    "application/json")),
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
            MockHttpClient.CreateSimpleSubstitute(HttpStatusCode.OK,
                new StringContent(JsonSerializer.Serialize(new FakeResponse {
                        Message = "Hello"
                    }), Encoding.UTF8,
                    "application/json")),
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
            MockHttpClient.CreateSimpleSubstitute(HttpStatusCode.OK,
                new StringContent("null", Encoding.UTF8, "application/json")),
            new FakeHybridCache(),
            _options);

        var requestModel = new FakeRequest();

        // Act
        var result = await client.Get<FakeRequest, FakeResponse>(requestModel);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should()
            .Be("Unable to deserialize the response into a JSON object, resulted in a null object.");
    }
}