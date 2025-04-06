using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class GetCharactersByPlayerRequest : GetRequestModel<GetCharactersByPlayerRequest> {
    public string PlayerName { get; init; } = string.Empty;

    public override string GetCacheKey() =>
        $"{nameof(GetCharactersByPlayerRequest)}_{PlayerName}";

    public override Dictionary<string, string> GetQueryParams() =>
        new() {
            { "player_name", PlayerName }
        };

    public override string GetEndpoint() =>
        "/get_chars_by_player";
}