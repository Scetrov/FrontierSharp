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

public class WorldApiClientTests {
    private const string ValidResponse = """
                                         {
                                             "data": [
                                                 { "id": 1, "name": "Type A" },
                                                 { "id": 2, "name": "Type B" }
                                             ],
                                             "metadata": {
                                                 "total": 2,
                                                 "limit": 100,
                                                 "offset": 0
                                             }
                                         }
                                         """;

    private const string PagedResponse = """
                                         {
                                             "data": [
                                                 { "id": 1, "name": "Type A" }
                                             ],
                                             "metadata": {
                                                 "total": 2,
                                                 "limit": 1,
                                                 "offset": 0
                                             }
                                         }
                                         """;

    private const string SecondPage = """
                                      {
                                          "data": [
                                              { "id": 2, "name": "Type B" }
                                          ],
                                          "metadata": {
                                              "total": 2,
                                              "limit": 1,
                                              "offset": 1
                                          }
                                      }
                                      """;

    private readonly HybridCache _cache = new FakeHybridCache();
    private readonly MockLogger<FrontierSharpHttpClient> _logger = Substitute.For<MockLogger<FrontierSharpHttpClient>>();
    private readonly IOptions<FrontierSharpHttpClientOptions> _options = Substitute.For<IOptions<FrontierSharpHttpClientOptions>>();

    public WorldApiClientTests() {
        _options.Value.Returns(new FrontierSharpHttpClientOptions {
            BaseUri = "https://test.local"
        });
    }

    private string TypesResourcePage1 => LoadResourceByPage("types", 100, 0);
    private string TypesResourcePage2 => LoadResourceByPage("types", 100, 100);
    private string TypesResourcePage3 => LoadResourceByPage("types", 100, 200);

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

    private string[] LoadResources(string endpoint, long limit, long maxValue) {
        return Enumerable.Range(0, (int)(maxValue / limit) + 1)
            .Select(i => LoadResourceByPage(endpoint, limit, i * limit))
            .ToArray();
    }

    [Fact]
    public async Task GetTypesPage_ShouldReturnTypes_WhenResponseIsValid() {
        // Arrange
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(ValidResponse);
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetTypesPage();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(2);
        result.Value.Metadata.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetTypesPage_ShouldReturnFailure_WhenInnerCallFails() {
        // Arrange
        var factory = SubstitutableHttpClientFactory.CreateInternalServerError();
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetTypesPage();

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task GetAllTypes_ShouldReturnAllPages_WhenMultiplePagesExist() {
        // Arrange
        var responses = new Queue<string>([PagedResponse, SecondPage]);

        var factory = new SubstitutableHttpClientFactory((_, _) => {
            var json = responses.Dequeue();
            var msg = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(msg);
        });

        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);

        // Act
        var result = await client.GetAllTypes(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(x => x.Name).Should().BeEquivalentTo("Type A", "Type B");
    }

    [Fact]
    public async Task GetAllSolarSystems_ShouldReturnAllPages_WhenMultiplePagesExist() {
        // Arrange
        var resources = LoadResources("solarsystems", 1000, 24502);
        resources.Length.Should().Be(25);
        resources.Last().Should().Contain("V-200");
        var responses = new Queue<string>(resources);

        var factory = new SubstitutableHttpClientFactory((_, _) => {
            var json = responses.Dequeue();
            var msg = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(msg);
        });

        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);

        // Act
        var result = await client.GetAllSolarSystems();

        // Assert
        result.Errors.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(24502);
    }

    [Fact]
    public async Task GetAllTypes_ShouldReturnAllPages_WhenMultiplePagesExist_WithRealData() {
        // Arrange
        var client = SetupApiClientWithResponses(TypesResourcePage1, TypesResourcePage2, TypesResourcePage3);

        // Act
        var result = await client.GetAllTypes();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(274);
    }

    [Fact]
    public async Task GetSolarSystemById_ShouldReturn_WithRealData() {
        // Arrange
        var payload = GetResourceString("FrontierSharp.Tests.WorldApi.payloads.v2.solarsystems.30012580.json");
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(payload);
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetSolarSystemById(30012580);

        // Assert
        result.Errors.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(30012580);
        result.Value.Name.Should().Be("USR-21H");
        result.Value.Location.Should().NotBeNull();
        result.Value.Location.X.Should().BeNegative();
        result.Value.Location.Y.Should().BeNegative();
        result.Value.Location.Z.Should().BePositive();
        result.Value.SmartAssemblies.Should().NotBeNull();
        result.Value.SmartAssemblies.Count().Should().Be(12);
        result.Value.RegionId.Should().Be(10000110);
    }

    private WorldApiClient CreateWorldApiClient(IHttpClientFactory factory) {
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);
        return client;
    }

    [Fact]
    public async Task GetAllTypes_ShouldReturnFailure_WhenPageFails() {
        // Arrange
        var factory = SubstitutableHttpClientFactory.CreateInternalServerError();
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);

        // Act
        var result = await client.GetAllTypes();

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task GetTypeById_ShouldReturnType_WhenResponseIsValid() {
        // Arrange
        var response = """
                       {
                           "id": 42,
                           "name": "Feral Data",
                           "description": "desc",
                           "mass": 1.5,
                           "radius": 2.5,
                           "volume": 3.5,
                           "portionSize": 4,
                           "groupId": 5,
                           "groupName": "Group",
                           "categoryId": 6,
                           "categoryName": "Category",
                           "iconUrl": "https://example/icon.png"
                       }
                       """;
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(response);
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetTypeById(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(42);
        result.Value.Name.Should().Be("Feral Data");
        result.Value.GroupName.Should().Be("Group");
        result.Value.CategoryName.Should().Be("Category");
    }

    [Fact]
    public async Task GetTypeById_ShouldReturnFailure_WhenInnerCallFails() {
        // Arrange
        var factory = SubstitutableHttpClientFactory.CreateInternalServerError();
        var client = CreateWorldApiClient(factory);

        // Act
        var result = await client.GetTypeById(42);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
    }

    private WorldApiClient SetupApiClientWithResponses(params string[] responses) {
        var queue = new Queue<string>(responses);
        var factory = new SubstitutableHttpClientFactory((_, _) => {
            var json = queue.Dequeue();
            var msg = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(msg);
        });

        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        return new WorldApiClient(httpClient);
    }
}