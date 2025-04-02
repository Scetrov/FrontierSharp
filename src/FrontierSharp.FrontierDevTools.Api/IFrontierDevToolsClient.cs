using FluentResults;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;

namespace FrontierSharp.FrontierDevTools.Api;

public interface IFrontierDevToolsClient {
    Task<IResult<CharactersResponse>> GetCharactersByName(string name, CancellationToken ct = default);
    Task<IResult<CharactersResponse>> GetCharactersByAddress(string address, CancellationToken ct = default);
    Task<IResult<CorpResponse>> GetCharactersByCorpId(int corpId, CancellationToken ct = default);
    Task<IResult<CorpResponse>> GetCharactersByPlayer(string playerName, CancellationToken ct = default);
}