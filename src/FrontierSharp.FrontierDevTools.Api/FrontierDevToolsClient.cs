using FluentResults;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using FrontierSharp.HttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace FrontierSharp.FrontierDevTools.Api;

public class FrontierDevToolsClient([FromKeyedServices(nameof(FrontierDevToolsClient))] IFrontierSharpHttpClient httpClient) : IFrontierDevToolsClient {
    public async Task<IResult<CharactersResponse>> GetCharactersByName(string name, CancellationToken ct = default) {
        return await httpClient.Get<GetCharacterByNameRequest, CharactersResponse>(
            new GetCharacterByNameRequest { PlayerName = name }, ct);
    }

    public async Task<IResult<CharactersResponse>> GetCharactersByAddress(string address, CancellationToken ct = default) {
        return await httpClient.Get<GetCharacterByAddressRequest, CharactersResponse>(
            new GetCharacterByAddressRequest { Address = address }, ct);
    }

    public async Task<IResult<CorpResponse>> GetCharactersByCorpId(int corpId, CancellationToken ct = default) {
        return await httpClient.Get<GetCharactersByCorpIdRequest, CorpResponse>(
            new GetCharactersByCorpIdRequest { CorpId = corpId }, ct);
    }

    public async Task<IResult<CorpResponse>> GetCharactersByPlayer(string playerName, CancellationToken ct = default) {
        return await httpClient.Get<GetCharactersByPlayerRequest, CorpResponse>(
            new GetCharactersByPlayerRequest { PlayerName = playerName }, ct);
    }

    public async Task<IResult<GateNetworkResponse>> GetGateNetwork(string identifier, CancellationToken ct = default) {
        return await httpClient.Get<GetGateNetworkRequest, GateNetworkResponse>(
            new GetGateNetworkRequest { Identifier = identifier }, ct);
    }
}