using FluentResults;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;

namespace FrontierSharp.FrontierDevTools.Api;

public interface IFrontierDevToolsClient {
    Task<IResult<CharactersResponse>> GetCharactersByName(string name, CancellationToken ct = default);
    Task<IResult<CharactersResponse>> GetCharactersByAddress(string address, CancellationToken ct = default);
    Task<IResult<CorporationResponse>> GetCharactersByCorpId(int corpId, CancellationToken ct = default);
    Task<IResult<CorporationResponse>> GetCharactersByPlayer(string playerName, CancellationToken ct = default);
    Task<IResult<GateNetworkResponse>> GetGateNetwork(string identifier, CancellationToken ct = default);
    Task<IResult<RouteResponse>> OptimizeStargateAndNetworkPlacement(string start, string end, decimal maxDistance, NpcAvoidanceLevel avoidanceLevel, CancellationToken ct = default);
    Task<IResult<RouteResponse>> FindTravelRoute(string start, string end, bool avoidGates = false, decimal maxDistance = 100m, CancellationToken ct = default);
    Task<IResult<DistanceResponse>> CalculateDistance(string systemA, string systemB, CancellationToken ct = default);
    Task<IResult<SystemsWithinDistanceResponse>> FindSystemsWithinDistance(string systemName, decimal maxDistance, CancellationToken ct = default);
}