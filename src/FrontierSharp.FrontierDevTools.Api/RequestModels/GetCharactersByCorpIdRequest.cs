using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class GetCharactersByCorpIdRequest : GetRequestModel<GetCharactersByCorpIdRequest> {
    public int CorpId { get; init; } = 0;

    public override string GetCacheKey() =>
        $"{nameof(GetCharactersByCorpIdRequest)}_{CorpId}";

    public override Dictionary<string, string> GetQueryParams() =>
        new() {
            { "corp_id", CorpId.ToString() }
        };

    public override string GetEndpoint() =>
        "/get_chars_by_corp_id";
}