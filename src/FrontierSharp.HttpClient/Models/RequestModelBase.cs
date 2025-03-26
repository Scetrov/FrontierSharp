namespace FrontierSharp.HttpClient.Models;

public abstract class RequestModelBase {
    public abstract Dictionary<string, string> GetQueryParams();
    public abstract string GetEndpoint();
}