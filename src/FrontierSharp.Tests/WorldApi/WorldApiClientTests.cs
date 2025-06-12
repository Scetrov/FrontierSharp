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

    private readonly MockLogger<WorldApiClient> _logger = Substitute.For<MockLogger<WorldApiClient>>();
    private readonly IOptions<FrontierSharpHttpClientOptions> _options = Substitute.For<IOptions<FrontierSharpHttpClientOptions>>();

    public WorldApiClientTests() {
        _options.Value.Returns(new FrontierSharpHttpClientOptions {
            BaseUri = "https://test.local"
        });
    }

    private string TypesResourcePage1 => LoadResource("types", 100, 0);
    private string TypesResourcePage2 => LoadResource("types", 100, 100);
    private string TypesResourcePage3 => LoadResource("types", 100, 200);

    private string FuelsResourcePage1 => LoadResource("fuels", 100, 0);

    private string LoadResource(string endpoint, long limit, long offset) {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"FrontierSharp.Tests.WorldApi.payloads.v2.{endpoint}_limit={limit}_offset={offset}.json";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [Fact]
    public async Task GetTypesPage_ShouldReturnTypes_WhenResponseIsValid() {
        // Arrange
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(ValidResponse);
        var client = new WorldApiClient(_logger, factory, _cache, _options);

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
        var client = new WorldApiClient(_logger, factory, _cache, _options);

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

        var client = new WorldApiClient(_logger, factory, _cache, _options);

        // Act
        var result = await client.GetAllTypes(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(x => x.Name).Should().BeEquivalentTo("Type A", "Type B");
    }

    [Fact]
    public async Task GetAllTypes_ShouldReturnAllPages_WhenMultiplePagesExist_WithRealData() {
        // Arrange
        var client = SetupApiClientWithResponses(TypesResourcePage1, TypesResourcePage2, TypesResourcePage3);

        // Act
        var result = await client.GetAllTypes(100);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(274);
    }

    [Fact]
    public async Task GetAllFuels_ShouldReturnAllPages_WhenMultiplePagesExist_WithRealData() {
        // Arrange
        var client = SetupApiClientWithResponses(FuelsResourcePage1);

        // Act
        var result = await client.GetAllFuels(100);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(6);
    }

    [Fact]
    public async Task GetAllTypes_ShouldReturnFailure_WhenPageFails() {
        // Arrange
        var factory = SubstitutableHttpClientFactory.CreateInternalServerError();
        var client = new WorldApiClient(_logger, factory, _cache, _options);

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
                           "mass": 0.1,
                           "radius": 1,
                           "volume": 0.1,
                           "portionSize": 1,
                           "groupName": "Group",
                           "groupId": 1,
                           "categoryName": "Category",
                           "categoryId": 2,
                           "iconUrl": ""
                       }
                       """;
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(response);
        var client = new WorldApiClient(_logger, factory, _cache, _options);

        // Act
        var result = await client.GetTypeById(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(42);
        result.Value.Name.Should().Be("Feral Data");
    }

    [Fact]
    public async Task GetTypeById_ShouldReturnFailure_WhenInnerCallFails() {
        // Arrange
        var factory = SubstitutableHttpClientFactory.CreateInternalServerError();
        var client = new WorldApiClient(_logger, factory, _cache, _options);

        // Act
        var result = await client.GetTypeById(42);

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
        return new WorldApiClient(_logger, factory, _cache, _options);
    }
}