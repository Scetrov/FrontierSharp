using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class GetCharactersByPlayerRequest : GetRequestModel<GetCharactersByPlayerRequest>, IGetRequestModel {
    public string PlayerName { get; init; } = string.Empty;

    public override string GetCacheKey() {
        return $"{nameof(GetCharactersByPlayerRequest)}_{PlayerName}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            { "player_name", PlayerName }
        };
    }

    public override string GetEndpoint() {
        return "/get_chars_by_player";
    }
}