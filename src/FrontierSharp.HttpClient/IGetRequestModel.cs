namespace FrontierSharp.HttpClient;

public interface IGetRequestModel {
    string GetCacheKey();
    Dictionary<string, string> GetQueryParams();
    string GetEndpoint();
}