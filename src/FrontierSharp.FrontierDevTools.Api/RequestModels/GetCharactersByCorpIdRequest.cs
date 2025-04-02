using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class GetCharactersByCorpIdRequest : GetRequestModel<GetCharactersByCorpIdRequest> {
    public int CorpId { get; init; } = 0;

    public override string GetCacheKey() {
        return $"{nameof(GetCharactersByCorpIdRequest)}_{CorpId}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            { "corp_id", CorpId.ToString() }
        };
    }

    public override string GetEndpoint() {
        return "/get_chars_by_corp_id";
    }
}