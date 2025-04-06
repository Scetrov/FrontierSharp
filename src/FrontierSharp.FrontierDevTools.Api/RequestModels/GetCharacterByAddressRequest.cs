using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class GetCharacterByAddressRequest : GetRequestModel<GetCharacterByAddressRequest> {
    public string Address { get; init; } = string.Empty;

    public override string GetCacheKey() =>
        $"{nameof(GetCharacterByAddressRequest)}_{Address}";

    public override Dictionary<string, string> GetQueryParams() =>
        new() {
            { "player_address", Address }
        };

    public override string GetEndpoint() =>
        "/get_character_by_address";
}