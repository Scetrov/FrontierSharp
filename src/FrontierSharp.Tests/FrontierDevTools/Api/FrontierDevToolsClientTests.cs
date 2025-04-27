using System.Text.Json.Nodes;
using FluentAssertions;
using FluentResults;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using FrontierSharp.HttpClient;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api;

public class FrontierDevToolsClientTests {
    private readonly IFrontierSharpHttpClient _httpClient = Substitute.For<IFrontierSharpHttpClient>();
    private readonly FrontierDevToolsClient _sut;

    public FrontierDevToolsClientTests() {
        _sut = new FrontierDevToolsClient(_httpClient);
    }

    [Fact]
    public async Task OptimalStargateNetworkAndDeployment_ShouldCallHttpClientWithCorrectRequestModel() {
        var httpClient = Substitute.For<IFrontierSharpHttpClient>();
        var mockResponse = Substitute.For<IResult<OptimalStargateNetworkAndDeploymentResponse>>();

        httpClient
            .Get<OptimalStargateNetworkAndDeploymentRequest, OptimalStargateNetworkAndDeploymentResponse>(
                Arg.Any<OptimalStargateNetworkAndDeploymentRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        var client = new FrontierDevToolsClient(httpClient);

        var result = await client.OptimalStargateNetworkAndDeployment(
            "Start-A",
            "End-B",
            250m,
            NpcAvoidanceLevel.Medium,
            true,
            "MockShip");

        result.Should().BeSameAs(mockResponse);

        await httpClient.Received(1)
            .Get<OptimalStargateNetworkAndDeploymentRequest, OptimalStargateNetworkAndDeploymentResponse>(
                Arg.Is<OptimalStargateNetworkAndDeploymentRequest>(r =>
                    r.StartName == "Start-A" &&
                    r.EndName == "End-B" &&
                    r.MaxStargateDistance == 250m &&
                    r.NpcAvoidanceLevel == NpcAvoidanceLevel.Medium &&
                    r.AvoidGates == true &&
                    r.IncludeShips == "MockShip"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCharactersByName_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CharactersResponse>>();
        const string name = "JaneDoe";

        _httpClient.Get<GetCharacterByNameRequest, CharactersResponse>(
            Arg.Is<GetCharacterByNameRequest>(r => r.PlayerName == name),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetCharactersByName(name);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetCharactersByAddress_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CharactersResponse>>();
        const string address = "0xabc123";

        _httpClient.Get<GetCharacterByAddressRequest, CharactersResponse>(
            Arg.Is<GetCharacterByAddressRequest>(r => r.Address == address),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetCharactersByAddress(address);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetCharactersByCorpId_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CorporationResponse>>();
        const int corpId = 99;

        _httpClient.Get<GetCharactersByCorpIdRequest, CorporationResponse>(
            Arg.Is<GetCharactersByCorpIdRequest>(r => r.CorpId == corpId),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetCharactersByCorpId(corpId);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetCharactersByPlayer_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CorporationResponse>>();
        const string playerName = "CommanderShepard";

        _httpClient.Get<GetCharactersByPlayerRequest, CorporationResponse>(
            Arg.Is<GetCharactersByPlayerRequest>(r => r.PlayerName == playerName),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetCharactersByPlayer(playerName);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetGateNetwork_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<GateNetworkResponse>>();
        const string identifier = "Sol";

        _httpClient.Get<GetGateNetworkRequest, GateNetworkResponse>(
            Arg.Is<GetGateNetworkRequest>(r => r.Identifier == identifier),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetGateNetwork(identifier);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task CalculateDistance_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<DistanceResponse>>();
        const string systemA = "EFN-12M";
        const string systemB = "H.BQL.581";

        _httpClient.Get<CalculateDistanceRequest, DistanceResponse>(
            Arg.Is<CalculateDistanceRequest>(r => r.SystemA == systemA && r.SystemB == systemB),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.CalculateDistance(systemA, systemB);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task FindSystemsWithinDistance_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<SystemsWithinDistanceResponse>>();
        const string systemName = "EFN-12M";
        const decimal maxDistance = 42.5m;

        _httpClient.Get<FindSystemsWithinDistanceRequest, SystemsWithinDistanceResponse>(
            Arg.Is<FindSystemsWithinDistanceRequest>(r => r.SystemName == systemName && r.MaxDistance == maxDistance),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.FindSystemsWithinDistance(systemName, maxDistance);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task OptimizeStargateAndNetworkPlacement_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<RouteResponse>>();
        const string start = "SOL";
        const string end = "ALPHA-CENT";
        const decimal maxDistance = 250m;
        const NpcAvoidanceLevel avoidanceLevel = NpcAvoidanceLevel.Medium;

        _httpClient.Get<OptimizeStargateNetworkPlacementRequest, RouteResponse>(
            Arg.Is<OptimizeStargateNetworkPlacementRequest>(r =>
                r.StartName == start &&
                r.EndName == end &&
                r.MaxDistanceInLightYears == maxDistance &&
                r.NpcAvoidanceLevel == avoidanceLevel),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.OptimizeStargateAndNetworkPlacement(start, end, maxDistance, avoidanceLevel);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task FindTravelRoute_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<RouteResponse>>();
        const string start = "NEBULA-1";
        const string end = "NEBULA-9";
        const bool avoidGates = true;
        const decimal maxDistance = 150m;

        _httpClient.Get<FindTravelRouteRequest, RouteResponse>(
            Arg.Is<FindTravelRouteRequest>(r =>
                r.StartName == start &&
                r.EndName == end &&
                r.AvoidGates == avoidGates &&
                r.MaxDistanceInLightYears == maxDistance),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.FindTravelRoute(start, end, avoidGates, maxDistance);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task CalculateTravelDistance_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<TravelDistanceResponse>>();
        const decimal currentFuel = 2800m;
        const decimal mass = 4795000m;
        const decimal fuelEfficiency = 80m;

        _httpClient.Get<CalculateTravelDistanceRequest, TravelDistanceResponse>(
            Arg.Is<CalculateTravelDistanceRequest>(r =>
                r.CurrentFuel == currentFuel &&
                r.Mass == mass &&
                r.FuelEfficiency == fuelEfficiency),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.CalculateTravelDistance(currentFuel, mass, fuelEfficiency);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task CalculateFuelRequired_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<FuelRequiredResponse>>();
        const decimal mass = 4795000m;
        const decimal lightyears = 99m;
        const decimal fuelEfficiency = 80m;

        _httpClient.Get<CalculateFuelRequired, FuelRequiredResponse>(
            Arg.Is<CalculateFuelRequired>(r =>
                r.Mass == mass &&
                r.Lightyears == lightyears &&
                r.FuelEfficiency == fuelEfficiency),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.CalculateFuelRequired(mass, lightyears, fuelEfficiency);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task CalculateFuelPerLightyear_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<FuelPerLightyearResponse>>();
        const decimal mass = 4795000m;
        const decimal fuelEfficiency = 80m;

        _httpClient.Get<CalculateFuelPerLightyear, FuelPerLightyearResponse>(
            Arg.Is<CalculateFuelPerLightyear>(r =>
                r.Mass == mass &&
                r.FuelEfficiency == fuelEfficiency),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.CalculateFuelPerLightyear(mass, fuelEfficiency);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task FindCommonSystemsWithinDistance_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CommonSystemsWithinDistanceResponse>>();
        const string systemA = "EFN-12M";
        const string systemB = "H.BQL.581";
        const decimal maxDistance = 42.5m;

        _httpClient.Get<FindCommonSystemsWithinDistanceRequest, CommonSystemsWithinDistanceResponse>(
            Arg.Is<FindCommonSystemsWithinDistanceRequest>(r =>
                r.SystemA == systemA &&
                r.SystemB == systemB &&
                r.MaxDistance == maxDistance),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.FindCommonSystemsWithinDistance(systemA, systemB, maxDistance);

        result.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(PathByRequestModel))]
    public async Task RequestModelsParameters_ShouldMatchOpenApiDefinition<T>(string route, IGetRequestModel model) {
        await using var jsonContent =
            ResourceHelper.GetEmbeddedResource("FrontierSharp.Tests.FrontierDevTools.openapi.json");
        var jsonDocument = await JsonNode.ParseAsync(jsonContent);
        var requestModel = jsonDocument?["paths"]?[route]?["get"]?["parameters"];

        if (requestModel == null)
            throw new InvalidOperationException(
                "The $.paths.{route}.get.parameters node was not found in the OpenAPI document.");

        var parameters = requestModel.AsArray().Select(x => x?["name"]?.GetValue<string>()).ToArray();
        model.GetEndpoint().Should().Be(route);
        model.GetQueryParams().Keys.Should().BeEquivalentTo(parameters);
        model.GetCacheKey().Should().StartWith(model.GetType().Name);
    }

    [Fact]
    public async Task ClientShouldImplementAllMethods() {
        await using var jsonContent =
            ResourceHelper.GetEmbeddedResource("FrontierSharp.Tests.FrontierDevTools.openapi.json");
        var jsonDocument = await JsonNode.ParseAsync(jsonContent);
        var paths = jsonDocument?["paths"] as JsonObject;
        var specificationPaths = paths?.Select(x => x.Key).Where(x => x != "/get_mud_table_data").ToList();
        var implementedPaths = PathByRequestModel().Select(x => x[0].ToString()).ToList();

        specificationPaths.Should().BeEquivalentTo(implementedPaths);
    }

    public static IEnumerable<object[]> PathByRequestModel() {
        yield return [
            "/optimal_stargate_network_and_deployment",
            new OptimalStargateNetworkAndDeploymentRequest {
                StartName = "A",
                EndName = "B",
                NpcAvoidanceLevel = NpcAvoidanceLevel.Medium,
                MaxStargateDistance = 99m,
                AvoidGates = true,
                IncludeShips = "Flegel"
            }
        ];

        yield return [
            "/optimize_stargate_network_placement",
            new OptimizeStargateNetworkPlacementRequest {
                StartName = "A",
                EndName = "B",
                NpcAvoidanceLevel = NpcAvoidanceLevel.Medium,
                MaxDistanceInLightYears = 99m
            }
        ];

        yield return [
            "/find_travel_route",
            new FindTravelRouteRequest {
                StartName = "A",
                EndName = "B",
                AvoidGates = true,
                MaxDistanceInLightYears = 99m
            }
        ];

        yield return [
            "/calculate_distance",
            new CalculateDistanceRequest {
                SystemA = "A",
                SystemB = "B"
            }
        ];

        yield return [
            "/find_systems_within_distance",
            new FindSystemsWithinDistanceRequest {
                SystemName = "A",
                MaxDistance = 99m
            }
        ];

        yield return [
            "/find_common_systems_within_distance",
            new FindCommonSystemsWithinDistanceRequest {
                SystemA = "A",
                SystemB = "B",
                MaxDistance = 99m
            }
        ];

        yield return [
            "/calculate_travel_distance",
            new CalculateTravelDistanceRequest {
                CurrentFuel = 2800,
                FuelEfficiency = 80,
                Mass = 4795000
            }
        ];

        yield return [
            "/calculate_fuel_required",
            new CalculateFuelRequired {
                Mass = 4795000,
                Lightyears = 99m,
                FuelEfficiency = 80
            }
        ];

        yield return [
            "/calculate_fuel_per_lightyear",
            new CalculateFuelPerLightyear {
                Mass = 4795000,
                FuelEfficiency = 80
            }
        ];

        yield return [
            "/get_character_by_name",
            new GetCharacterByNameRequest {
                PlayerName = "A"
            }
        ];

        yield return [
            "/get_character_by_address",
            new GetCharacterByAddressRequest {
                Address = "A"
            }
        ];

        yield return [
            "/get_chars_by_corp_id",
            new GetCharactersByCorpIdRequest {
                CorpId = 99
            }
        ];

        yield return [
            "/get_chars_by_player",
            new GetCharactersByPlayerRequest {
                PlayerName = "A"
            }
        ];

        yield return [
            "/get_gate_network",
            new GetGateNetworkRequest {
                Identifier = "A"
            }
        ];
    }
}