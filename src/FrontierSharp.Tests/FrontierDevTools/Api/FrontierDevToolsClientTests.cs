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
    public async Task GetCharactersByName_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CharactersResponse>>();
        var name = "JaneDoe";

        _httpClient.Get<GetCharacterByNameRequest, CharactersResponse>(
            Arg.Is<GetCharacterByNameRequest>(r => r.PlayerName == name),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetCharactersByName(name);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetCharactersByAddress_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CharactersResponse>>();
        var address = "0xabc123";

        _httpClient.Get<GetCharacterByAddressRequest, CharactersResponse>(
            Arg.Is<GetCharacterByAddressRequest>(r => r.Address == address),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetCharactersByAddress(address);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetCharactersByCorpId_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CorporationResponse>>();
        var corpId = 99;

        _httpClient.Get<GetCharactersByCorpIdRequest, CorporationResponse>(
            Arg.Is<GetCharactersByCorpIdRequest>(r => r.CorpId == corpId),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetCharactersByCorpId(corpId);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetCharactersByPlayer_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CorporationResponse>>();
        var playerName = "CommanderShepard";

        _httpClient.Get<GetCharactersByPlayerRequest, CorporationResponse>(
            Arg.Is<GetCharactersByPlayerRequest>(r => r.PlayerName == playerName),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetCharactersByPlayer(playerName);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetGateNetwork_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<GateNetworkResponse>>();
        var identifier = "Sol";

        _httpClient.Get<GetGateNetworkRequest, GateNetworkResponse>(
            Arg.Is<GetGateNetworkRequest>(r => r.Identifier == identifier),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetGateNetwork(identifier);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task CalculateDistance_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<DistanceResponse>>();
        var systemA = "EFN-12M";
        var systemB = "H.BQL.581";

        _httpClient.Get<CalculateDistanceRequest, DistanceResponse>(
            Arg.Is<CalculateDistanceRequest>(r => r.SystemA == systemA && r.SystemB == systemB),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.CalculateDistance(systemA, systemB);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task FindSystemsWithinDistance_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<SystemsWithinDistanceResponse>>();
        var systemName = "EFN-12M";
        var maxDistance = 42.5m;

        _httpClient.Get<FindSystemsWithinDistanceRequest, SystemsWithinDistanceResponse>(
            Arg.Is<FindSystemsWithinDistanceRequest>(r => r.SystemName == systemName && r.MaxDistance == maxDistance),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.FindSystemsWithinDistance(systemName, maxDistance);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task OptimalStargateAndNetworkPlacement_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<RouteResponse>>();
        var start = "SOL";
        var end = "ALPHA-CENT";
        var maxDistance = 250m;
        var avoidanceLevel = NpcAvoidanceLevel.Medium;

        _httpClient.Get<OptimizeStargateNetworkPlacementRequest, RouteResponse>(
            Arg.Is<OptimizeStargateNetworkPlacementRequest>(r =>
                r.StartName == start &&
                r.EndName == end &&
                r.MaxDistanceInLightYears == maxDistance &&
                r.NpcAvoidanceLevel == avoidanceLevel),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.OptimalStargateAndNetworkPlacement(start, end, maxDistance, avoidanceLevel);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task FindTravelRoute_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<RouteResponse>>();
        var start = "NEBULA-1";
        var end = "NEBULA-9";
        var avoidGates = true;
        var maxDistance = 150m;

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


    [Theory]
    [MemberData(nameof(GetFindTravelRouteTestCases))]
    public async Task RequestModelsParameters_ShouldMatchOpenApiDefinition<T>(string route, IGetRequestModel model) {
        await using var jsonContent = ResourceHelper.GetEmbeddedResource("FrontierSharp.Tests.FrontierDevTools.openapi.json");
        var jsonDocument = await JsonNode.ParseAsync(jsonContent);
        var requestModel = jsonDocument?["paths"]?[route]?["get"]?["parameters"];

        if (requestModel == null) {
            throw new InvalidOperationException("The $.paths.{route}.get.parameters node was not found in the OpenAPI document.");
        }

        var parameters = requestModel.AsArray().Select(x => x?["name"]?.GetValue<string>()).ToArray();
        model.GetEndpoint().Should().Be(route);
        model.GetQueryParams().Keys.Should().BeEquivalentTo(parameters);
    }

    public static IEnumerable<object[]> GetFindTravelRouteTestCases() {
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