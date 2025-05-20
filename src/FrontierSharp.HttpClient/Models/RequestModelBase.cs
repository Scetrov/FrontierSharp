namespace FrontierSharp.HttpClient.Models;

public abstract class RequestModelBase {
    public abstract string GetCacheKey();
    public abstract Dictionary<string, string> GetQueryParams();
    public abstract string GetEndpoint();
}