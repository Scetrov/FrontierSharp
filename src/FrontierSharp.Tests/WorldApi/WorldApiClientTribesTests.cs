// filepath: c:\source\FrontierSharp\src\FrontierSharp.Tests\WorldApi\WorldApiClientTribesTests.cs

using System.Net;
using System.Reflection;
using System.Text;
using AwesomeAssertions;
using FrontierSharp.HttpClient;
using FrontierSharp.WorldApi;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.WorldApi;

public class WorldApiClientTribesTests {
    private readonly HybridCache _cache = new FakeHybridCache();
    private readonly MockLogger<FrontierSharpHttpClient> _logger = Substitute.For<MockLogger<FrontierSharpHttpClient>>();
    private readonly IOptions<FrontierSharpHttpClientOptions> _options = Substitute.For<IOptions<FrontierSharpHttpClientOptions>>();

    public WorldApiClientTribesTests() {
        _options.Value.Returns(new FrontierSharpHttpClientOptions {
            BaseUri = "https://test.local"
        });
    }

    private string LoadResourceByPage(string endpoint, long limit, long offset) {
        var resourceName = $"FrontierSharp.Tests.WorldApi.payloads.v2.{endpoint}_limit={limit}_offset={offset}.json";
        return GetResourceString(resourceName);
    }

    private static string GetResourceString(string resourceName) {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private WorldApiClient CreateWorldApiClient(IHttpClientFactory factory) {
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);
        return client;
    }

    [Fact]
    public async Task GetTribesPage_ShouldReturnTribes_WhenResponseIsValid() {
        // Arrange
        var payload = LoadResourceByPage("tribes", 100, 0);
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(payload);
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetTribesPage();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().NotBeEmpty();
        result.Value.Metadata.Total.Should().Be(38);
        var first = result.Value.Data.First();
        first.Id.Should().Be(98000053);
        first.Name.Should().Be("C C P");
    }

    [Fact]
    public async Task GetTribesPage_ShouldReturnFailure_WhenInnerCallFails() {
        // Arrange
        var factory = SubstitutableHttpClientFactory.CreateInternalServerError();
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetTribesPage();

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task GetAllTribes_ShouldReturnAllPages_WithRealData() {
        // Arrange
        var client = SetupApiClientWithResponses(LoadResourceByPage("tribes", 100, 0));

        // Act
        var result = await client.GetAllTribes();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(38);
    }

    [Fact]
    public async Task GetTribeById_ShouldReturnTribeDetail_WhenResponseIsValid() {
        // Arrange
        var payload = GetResourceString("FrontierSharp.Tests.WorldApi.payloads.tribes.98000314.json");
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(payload);
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetTribeById(98000314);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(98000314);
        result.Value.Name.Should().Be("REAPERS");
        result.Value.NameShort.Should().Be("REAP");
        result.Value.MemberCount.Should().Be(33);
        result.Value.Members.Should().HaveCount(33);
        var firstMember = result.Value.Members.First();
        firstMember.Address.Should().Be("0x87e015b96fe7cce7915b1fe893e153eefe0f6eb2");
        firstMember.Name.Should().Be("Jabolko");
    }

    [Fact]
    public async Task GetTribeById_ShouldReturnFailure_WhenInnerCallFails() {
        // Arrange
        var factory = SubstitutableHttpClientFactory.CreateInternalServerError();
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetTribeById(98000314);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
    }

    private WorldApiClient SetupApiClientWithResponses(params string[] responses) {
        var responseQueue = new Queue<string>(responses);
        var factory = new SubstitutableHttpClientFactory((_, _) => {
            var json = responseQueue.Dequeue();
            var msg = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(msg);
        });
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        return new WorldApiClient(httpClient);
    }
}