// filepath: c:\source\FrontierSharp\src\FrontierSharp.WorldApi\RequestModel\GetTribeById.cs
using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetTribeById : GetRequestModel<GetTribeById>, IGetRequestModel {
    public long TribeId { get; set; }

    public override string GetCacheKey() {
        return $"WorldApi_Tribe_{TribeId}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string>();
    }

    public override string GetEndpoint() => $"/v2/tribes/{TribeId}";
}
