using System.Net;
using System.Numerics;
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

    private string FuelsResourcePage1 => LoadResourceByPage("fuels", 100, 0);

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
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);

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
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);

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
    public async Task GetAllSmartAssemblies_ShouldReturnAllPages_WhenMultiplePagesExist_WithRealData() {
        // Arrange
        var client = SetupApiClientWithResponses(LoadResources("smartassemblies", 100, 13200));

        // Act
        var result = await client.GetAllSmartAssemblies();

        // Assert
        result.Errors.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(13212);
        var first = result.Value.First();
        first.Id.Should().Be("75343970651982257052710820829442849942642924970878978184835257992027850797979");
        first.Type.Should().Be(SmartAssemblyType.Manufacturing);
        first.Name.Should().BeEmpty();
        first.SolarSystem.Should().NotBeNull();
        first.SolarSystem.Id.Should().Be(30012580);
        first.SolarSystem.Name.Should().Be("USR-21H");
        first.SolarSystem.Location.X.Should().BeNegative();
        first.SolarSystem.Location.Y.Should().BeNegative();
        first.SolarSystem.Location.Z.Should().BePositive();
        first.Owner.Address.Should().Be("0x69898f12f920832458609442d5afab9ba68887f6");
        first.Owner.Name.Should().Be("Bull");
        first.Owner.Id.Should().Be("84599722058431940285285062988086044414659287579074488044166947650185462704496");
        first.EnergyUsage.Should().Be(0);
        first.TypeId.Should().Be(87161);
    }

    [Fact]
    public async Task GetAllSmartCharacters_ShouldReturnAllPages_WhenMultiplePagesExist_WithRealData() {
        // Arrange
        var client = SetupApiClientWithResponses(LoadResources("smartcharacters", 100, 1900));

        // Act
        var result = await client.GetAllSmartCharacters();

        // Assert
        result.Errors.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1993);
        result.Value.First().Id.Should().Be("75486957380819201602640431759284479289952881287900147001839740700576139323212");
        result.Value.First().Name.Should().Be("vookid");
        result.Value.First().Address.Should().Be("0xcda43b6f62c3ccebdaf50afe2b9b1b46e196581a");
    }

    [Fact]
    public async Task GetAllFuels_ShouldReturnAllPages_WhenMultiplePagesExist_WithRealData() {
        // Arrange
        var client = SetupApiClientWithResponses(FuelsResourcePage1);

        // Act
        var result = await client.GetAllFuels();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(6);
        var firstResult = result.Value.First();
        firstResult.Type.Name.Should().Be("EU-90 Fuel");
        firstResult.Type.Id.Should().Be(78437);
        firstResult.Type.GroupName.Should().Be("Crude Fuel");
        firstResult.Type.CategoryName.Should().Be("Commodity");
        firstResult.Type.Description.Should().NotBe(null);
        firstResult.Type.Mass.Should().Be(30.0);
        firstResult.Type.Volume.Should().BeGreaterThan(0);
        firstResult.Type.PortionSize.Should().Be(357143);
        firstResult.Type.IconUrl.Should().Be("https://artifacts.evefrontier.com/types/78437.png");
        firstResult.Efficiency.Should().Be(90);
    }

    [Fact]
    public async Task GetSolarSystemById_ShouldReturn_WithRealData() {
        // Arrange
        var payload = GetResourceString("FrontierSharp.Tests.WorldApi.payloads.v2.solarsystems.30012580.json");
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(payload);
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);

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

    [Fact]
    public async Task GetSmartCharacterById_ShouldReturn_WithRealData() {
        // Arrange
        var payload = GetResourceString("FrontierSharp.Tests.WorldApi.payloads.v2.smartcharacters.0x19957f367b81bd7711d316a451ade0d8fa8cb5bf.json");
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(payload);
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);

        // Act
        var result = await client.GetSmartCharacterById("0x19957f367b81bd7711d316a451ade0d8fa8cb5bf");

        // Assert
        result.Errors.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be("89979072328616633636445914640704752155006469856397668317334881359961980144528");
        result.Value.Name.Should().Be("vayan");
        result.Value.Address.Should().Be("0x19957f367b81bd7711d316a451ade0d8fa8cb5bf");
        result.Value.TribeId.Should().Be(98000001);
        result.Value.EveBalanceInWei.Should().Be(10000000000000000);
        result.Value.GasBalanceInWei.Should().Be(1000000000000000);
        result.Value.SmartAssemblies.Should().BeEmpty();
        result.Value.PortraitUrl.Should().Be("https://artifacts.evefrontier.com/portraits/PortraitAscended256.png");
    }

    [Fact]
    public async Task GetSmartAssemblyById_ShouldReturn_WithRealData() {
        // Arrange
        var payload = GetResourceString("FrontierSharp.Tests.WorldApi.payloads.v2.smartassemblies.75343970651982257052710820829442849942642924970878978184835257992027850797979.json");
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(payload);
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);

        // Act
        var result = await client.GetSmartAssemblyById(BigInteger.Parse("75343970651982257052710820829442849942642924970878978184835257992027850797979"));

        // Assert
        result.Errors.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Should().NotBeNull();
        result.Value.Id.Should().Be("75343970651982257052710820829442849942642924970878978184835257992027850797979");
        result.Value.Type.Should().Be(SmartAssemblyType.Manufacturing);
        result.Value.Name.Should().BeEmpty();
        result.Value.State.Should().Be(SmartAssemblyState.Anchored);

        result.Value.SolarSystem.Should().NotBeNull();
        result.Value.SolarSystem.Id.Should().Be(30012580);
        result.Value.SolarSystem.Name.Should().Be("USR-21H");
        result.Value.SolarSystem.Location.Should().NotBeNull();
        result.Value.SolarSystem.Location.X.Should().Be(-3446295502205747000);
        result.Value.SolarSystem.Location.Y.Should().Be(-100055051321475070);
        result.Value.SolarSystem.Location.Z.Should().Be(9596258211275473000);

        result.Value.Owner.Should().NotBeNull();
        result.Value.Owner.Address.Should().Be("0x69898f12f920832458609442d5afab9ba68887f6");
        result.Value.Owner.Name.Should().Be("Bull");
        result.Value.Owner.Id.Should().Be("84599722058431940285285062988086044414659287579074488044166947650185462704496");

        result.Value.EnergyUsage.Should().Be(0);
        result.Value.TypeId.Should().Be(87161);

        result.Value.TypeDetails.Should().NotBeNull();
        result.Value.TypeDetails.Id.Should().Be(87161);
        result.Value.TypeDetails.Name.Should().Be("Portable Refinery");
        result.Value.TypeDetails.Description.Should().Be("Cheap and portable refinery that goes anywhere with you.");
        result.Value.TypeDetails.Mass.Should().Be(22200000);
        result.Value.TypeDetails.Radius.Should().Be(1);
        result.Value.TypeDetails.Volume.Should().Be(10000);
        result.Value.TypeDetails.PortionSize.Should().Be(1);
        result.Value.TypeDetails.GroupName.Should().Be("Core");
        result.Value.TypeDetails.GroupId.Should().Be(0);
        result.Value.TypeDetails.CategoryName.Should().Be("Deployable");
        result.Value.TypeDetails.CategoryId.Should().Be(22);
        result.Value.TypeDetails.IconUrl.Should().BeEmpty();

        result.Value.Description.Should().BeEmpty();
        result.Value.DappUrl.Should().BeEmpty();

        result.Value.Manufacturing.Should().NotBeNull();
        result.Value.Manufacturing.IsParentNodeOnline.Should().BeFalse();

        result.Value.Location.Should().NotBeNull();
        result.Value.Location.X.Should().Be(121834326528);
        result.Value.Location.Y.Should().Be(-5201916416);
        result.Value.Location.Z.Should().Be(241142638592);
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
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);

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
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);

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
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        return new WorldApiClient(httpClient);
    }
}