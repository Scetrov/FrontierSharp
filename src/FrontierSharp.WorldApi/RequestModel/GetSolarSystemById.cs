using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetSolarSystemById : GetRequestModel<GetSolarSystemById>, IGetRequestModel {
    public long SolarSystemId { get; set; }

    public override string GetCacheKey() {
        return $"WorldApi_SolarSystem_{SolarSystemId}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string>();
    }

    public override string GetEndpoint() {
        return $"/v2/types/{SolarSystemId}";
    }
}