using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetSmartCharacterById : GetRequestModel<GetSmartCharacterById>, IGetRequestModel {
    public string CharacterAddress { get; init; } = string.Empty;

    public override string GetCacheKey() {
        return $"WorldApi_Type_{CharacterAddress}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string>();
    }

    public override string GetEndpoint() {
        return $"/v2/smartcharacters/{CharacterAddress}";
    }
}