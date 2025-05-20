using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using FrontierSharp.HttpClient;
using FrontierSharp.Tests.Utils.FakeHttpClientFactory;
using FrontierSharp.Tests.Utils.FakeHybridCache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.HttpClient;

public class FrontierSharpHttpPostTests {
    private readonly IOptions<FrontierSharpHttpClientOptions> _options;

    public FrontierSharpHttpPostTests() {
        var options = new FrontierSharpHttpClientOptions {
            BaseUri = "http://localhost",
            HttpClientName = "TestClient"
        };
        _options = Options.Create(options);
    }

    [Fact]
    public async Task Post_ReturnsFailure_WhenResponseIsNotSuccessful() {
        var client = new FrontierSharpHttpClient(
            Substitute.For<ILogger<FrontierSharpHttpClient>>(),
            MockHttpClient.CreateSimpleSubstitute(HttpStatusCode.BadRequest),
            new FakeHybridCache(),
            _options);

        var requestModel = new FakePostRequest();

        var result = await client.Post<FakePostRequest, FakeResponse>(requestModel);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Request failed with status code 400 (Bad request).");
    }

    [Fact]
    public async Task Post_ReturnsSuccess_WhenResponseIsSuccessful_AndDeserializationSucceeds() {
        var json = JsonSerializer.Serialize(new FakeResponse {
            Message = "Posted!"
        });

        var client = new FrontierSharpHttpClient(
            Substitute.For<ILogger<FrontierSharpHttpClient>>(),
            MockHttpClient.CreateSimpleSubstitute(HttpStatusCode.OK,
                new StringContent(json, Encoding.UTF8, "application/json")),
            new FakeHybridCache(),
            _options);

        var requestModel = new FakePostRequest();

        var result = await client.Post<FakePostRequest, FakeResponse>(requestModel);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Message.Should().Be("Posted!");
    }

    [Fact]
    public async Task Post_ReturnsFailure_WhenDeserializationFails() {
        var client = new FrontierSharpHttpClient(
            Substitute.For<ILogger<FrontierSharpHttpClient>>(),
            MockHttpClient.CreateSimpleSubstitute(HttpStatusCode.OK,
                new StringContent("null", Encoding.UTF8, "application/json")),
            new FakeHybridCache(),
            _options);

        var requestModel = new FakePostRequest();

        var result = await client.Post<FakePostRequest, FakeResponse>(requestModel);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be(
            "Unable to deserialize the response into a JSON object, resulted in a null object.");
    }
}
