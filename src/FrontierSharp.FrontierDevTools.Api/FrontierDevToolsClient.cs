using FluentResults;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using FrontierSharp.HttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace FrontierSharp.FrontierDevTools.Api;

public class FrontierDevToolsClient([FromKeyedServices(nameof(FrontierDevToolsClient))] IFrontierSharpHttpClient httpClient) : IFrontierDevToolsClient {
    // OptimialStargateNetworkAndDeployment
    
    public async Task<IResult<RouteResponse>> OptimizeStargateAndNetworkPlacement(string start, string end, decimal maxDistance = 499m, NpcAvoidanceLevel avoidanceLevel = NpcAvoidanceLevel.High, CancellationToken ct = default) {
        return await httpClient.Get<OptimizeStargateNetworkPlacementRequest, RouteResponse>(
            new OptimizeStargateNetworkPlacementRequest { StartName = start, EndName = end, MaxDistanceInLightYears = maxDistance, NpcAvoidanceLevel = avoidanceLevel }, ct);
    }

    public async Task<IResult<RouteResponse>> FindTravelRoute(string start, string end, bool avoidGates = false, decimal maxDistance = 100m, CancellationToken ct = default) {
        return await httpClient.Get<FindTravelRouteRequest, RouteResponse>(
            new FindTravelRouteRequest { StartName = start, EndName = end, AvoidGates = avoidGates, MaxDistanceInLightYears = maxDistance }, ct);
    }

    public async Task<IResult<DistanceResponse>> CalculateDistance(string systemA, string systemB, CancellationToken ct = default) {
        return await httpClient.Get<CalculateDistanceRequest, DistanceResponse>(
            new CalculateDistanceRequest { SystemA = systemA, SystemB = systemB }, ct);
    }

    public async Task<IResult<SystemsWithinDistanceResponse>> FindSystemsWithinDistance(string systemName, decimal maxDistance, CancellationToken ct = default) {
        return await httpClient.Get<FindSystemsWithinDistanceRequest, SystemsWithinDistanceResponse>(
            new FindSystemsWithinDistanceRequest { SystemName = systemName, MaxDistance = maxDistance }, ct);
    }
    
    public async Task<IResult<CommonSystemsWithinDistanceResponse>> FindCommonSystemsWithinDistance(string systemA, string systemB, decimal maxDistance, CancellationToken ct = default) {
        return await httpClient.Get<FindCommonSystemsWithinDistanceRequest, CommonSystemsWithinDistanceResponse>(
            new FindCommonSystemsWithinDistanceRequest { SystemA = systemA, SystemB = systemB, MaxDistance = maxDistance }, ct);
    }
    
    public async Task<IResult<TravelDistanceResponse>> CalculateTravelDistance(decimal currentFuel, decimal mass, decimal fuelEfficency, CancellationToken ct = default) {
        return await httpClient.Get<CalculateTravelDistanceRequest, TravelDistanceResponse>(
            new CalculateTravelDistanceRequest { CurrentFuel = currentFuel, Mass = mass, FuelEfficiency = fuelEfficency}, ct);
    }
    
    public async Task<IResult<FuelRequiredResponse>> CalculateFuelRequired(decimal mass, decimal lightyears, decimal fuelEfficiency, CancellationToken ct = default) {
        return await httpClient.Get<CalculateFuelRequired, FuelRequiredResponse>(
            new CalculateFuelRequired { Mass = mass, Lightyears = lightyears, FuelEfficiency = fuelEfficiency }, ct);
    }
    
    public async Task<IResult<FuelPerLightyearResponse>> CalculateFuelPerLightyear(decimal mass, decimal fuelEfficiency, CancellationToken ct = default) {
        return await httpClient.Get<CalculateFuelPerLightyear, FuelPerLightyearResponse>(
            new CalculateFuelPerLightyear { Mass = mass, FuelEfficiency = fuelEfficiency}, ct);
    }
    
    public async Task<IResult<CharactersResponse>> GetCharactersByName(string name, CancellationToken ct = default) {
        return await httpClient.Get<GetCharacterByNameRequest, CharactersResponse>(
            new GetCharacterByNameRequest { PlayerName = name }, ct);
    }

    public async Task<IResult<CharactersResponse>> GetCharactersByAddress(string address, CancellationToken ct = default) {
        return await httpClient.Get<GetCharacterByAddressRequest, CharactersResponse>(
            new GetCharacterByAddressRequest { Address = address }, ct);
    }

    public async Task<IResult<CorporationResponse>> GetCharactersByCorpId(int corpId, CancellationToken ct = default) {
        return await httpClient.Get<GetCharactersByCorpIdRequest, CorporationResponse>(
            new GetCharactersByCorpIdRequest { CorpId = corpId }, ct);
    }

    public async Task<IResult<CorporationResponse>> GetCharactersByPlayer(string playerName, CancellationToken ct = default) {
        return await httpClient.Get<GetCharactersByPlayerRequest, CorporationResponse>(
            new GetCharactersByPlayerRequest { PlayerName = playerName }, ct);
    }
    
    public async Task<IResult<GateNetworkResponse>> GetGateNetwork(string identifier, CancellationToken ct = default) {
        return await httpClient.Get<GetGateNetworkRequest, GateNetworkResponse>(
            new GetGateNetworkRequest { Identifier = identifier }, ct);
    }
}