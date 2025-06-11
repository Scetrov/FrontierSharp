using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetTypeById : GetRequestModel<GetTypeById>, IGetRequestModel {
    public long TypeId { get; set; }

    public override string GetCacheKey() => $"WorldApi_Type_{TypeId}";

    public override Dictionary<string, string> GetQueryParams() => new();

    public override string GetEndpoint() => $"/v2/types/{TypeId}";
}