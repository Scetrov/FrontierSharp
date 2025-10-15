// filepath: c:\source\FrontierSharp\src\FrontierSharp.Tests\WorldApi\WorldApiClientSingleEntityTests.cs
using System.Net;
using System.Reflection;
using System.Text;
using AwesomeAssertions;
using FrontierSharp.HttpClient;
using FrontierSharp.WorldApi;
using FrontierSharp.WorldApi.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.WorldApi;

public class WorldApiClientSingleEntityTests {
    private readonly HybridCache _cache = new FakeHybridCache();
    private readonly MockLogger<FrontierSharpHttpClient> _logger = Substitute.For<MockLogger<FrontierSharpHttpClient>>();
    private readonly IOptions<FrontierSharpHttpClientOptions> _options = Substitute.For<IOptions<FrontierSharpHttpClientOptions>>();

    public WorldApiClientSingleEntityTests() {
        _options.Value.Returns(new FrontierSharpHttpClientOptions {
            BaseUri = "https://test.local"
        });
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
        return new WorldApiClient(httpClient);
    }

    [Fact]
    public async Task GetKillmailById_ShouldReturnKillmail_WhenResponseIsValid() {
        // Arrange
        var payload = GetResourceString("FrontierSharp.Tests.WorldApi.payloads.v2.killmails.1745854210705.json");
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(payload);
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetKillmailById("1745854210705");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Victim.Name.Should().Be("vayan");
        result.Value.Killer.Name.Should().Be("New Era");
        result.Value.SolarSystemId.Should().Be(30012542);
    }

    [Fact]
    public async Task GetKillmailById_ShouldReturnFailure_WhenInnerCallFails() {
        // Arrange
        var factory = SubstitutableHttpClientFactory.CreateInternalServerError();
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetKillmailById("1745854210705");

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task GetTribeById_ShouldReturnTribe_WhenResponseIsValid() {
        // Arrange
        var payload = GetResourceString("FrontierSharp.Tests.WorldApi.payloads.v2.tribes.98000053.json");
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(payload);
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetTribeById(98000053);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(98000053);
        result.Value.Name.Should().Be("C C P");
        result.Value.MemberCount.Should().Be(4);
    }

    [Fact]
    public async Task GetTribeById_ShouldReturnFailure_WhenInnerCallFails() {
        // Arrange
        var factory = SubstitutableHttpClientFactory.CreateInternalServerError();
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetTribeById(98000053);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task VerifyPod_ShouldReturnSuccess_WhenResponseIsValid() {
        // Arrange
        var payload = GetResourceString("FrontierSharp.Tests.WorldApi.payloads.v2.podverify_response.json");
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(payload);
        var client = CreateWorldApiClient(factory);
        var podData = new { test = "data" };

        // Act
        var result = await client.VerifyPod(podData);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Result.Should().BeTrue();
        result.Value.Error.Should().Be("none");
    }

    [Fact]
    public async Task VerifyPod_ShouldReturnFailure_WhenInnerCallFails() {
        // Arrange
        var factory = SubstitutableHttpClientFactory.CreateInternalServerError();
        var client = CreateWorldApiClient(factory);
        var podData = new { test = "data" };

        // Act
        var result = await client.VerifyPod(podData);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
    }
}
