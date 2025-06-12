using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetTypeById : GetRequestModel<GetTypeById>, IGetRequestModel {
    public long TypeId { get; set; }

    public override string GetCacheKey() {
        return $"WorldApi_Type_{TypeId}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string>();
    }

    public override string GetEndpoint() {
        return $"/v2/types/{TypeId}";
    }
}