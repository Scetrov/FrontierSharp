using FluentResults;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;

namespace FrontierSharp.FrontierDevTools.Api;

public interface IFrontierDevToolsClient {
    Task<IResult<CharactersResponse>> GetCharactersByName(string name, CancellationToken ct);
    Task<IResult<CharactersResponse>> GetCharactersByAddress(string address, CancellationToken ct);
}