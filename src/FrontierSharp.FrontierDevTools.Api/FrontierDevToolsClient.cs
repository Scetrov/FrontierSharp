using FluentResults;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using FrontierSharp.HttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace FrontierSharp.FrontierDevTools.Api;

public class FrontierDevToolsClient([FromKeyedServices(nameof(FrontierDevToolsClient))] IFrontierSharpHttpClient httpClient) : IFrontierDevToolsClient {
    public async Task<IResult<CharactersResponse>> GetCharactersByName(string name, CancellationToken ct) {
        return await httpClient.Get<GetCharacterByNameRequest, CharactersResponse>(
            new GetCharacterByNameRequest { PlayerName = name }, ct);
    }

    public async Task<IResult<CharactersResponse>> GetCharactersByAddress(string address, CancellationToken ct) {
        return await httpClient.Get<GetCharacterByAddressRequest, CharactersResponse>(
            new GetCharacterByAddressRequest { Address = address }, ct);
    }
}