using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class GetCharacterByNameRequest : GetRequestModel<GetCharacterByNameRequest>, IGetRequestModel {
    public string PlayerName { get; init; } = string.Empty;

    public override string GetCacheKey() {
        return $"{nameof(GetCharacterByNameRequest)}_{PlayerName}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            { "player_name", PlayerName }
        };
    }

    public override string GetEndpoint() {
        return "/get_character_by_name";
    }
}