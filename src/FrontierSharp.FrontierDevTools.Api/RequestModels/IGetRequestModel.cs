namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public interface IGetRequestModel {
    string GetCacheKey();
    Dictionary<string, string> GetQueryParams();
    string GetEndpoint();
}