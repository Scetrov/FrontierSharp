using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class GetCharacterByNameRequest : GetRequestModel<GetCharacterByNameRequest> {
    public string PlayerName { get; init; } = string.Empty;

    public override string GetCacheKey() =>
        $"{nameof(GetCharacterByNameRequest)}_{PlayerName}";

    public override Dictionary<string, string> GetQueryParams() =>
        new() {
            { "player_name", PlayerName }
        };

    public override string GetEndpoint() =>
        "/get_character_by_name";
}