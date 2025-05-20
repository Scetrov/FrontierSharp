using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.Tests.HttpClient;

public class FakeGetRequest : GetRequestModel<FakeGetRequest> {
    public override string GetCacheKey() {
        return "FakeRequestCacheKey";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string>();
    }

    public override string GetEndpoint() {
        return "/test";
    }
}