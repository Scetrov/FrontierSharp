using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class GetCharacterByAddressRequest : GetRequestModel<GetCharacterByAddressRequest>, IGetRequestModel {
    public string Address { get; init; } = string.Empty;

    public override string GetCacheKey() {
        return $"{nameof(GetCharacterByAddressRequest)}_{Address}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            {
                "player_address", Address
            }
        };
    }

    public override string GetEndpoint() {
        return "/get_character_by_address";
    }
}