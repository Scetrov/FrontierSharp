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
    Task<IResult<RouteResponse>> OptimalStargateAndNetworkPlacement(string start, string end, decimal maxDistance, NPCAvodianceLevel avoidanceLevel, CancellationToken ct = default);
}