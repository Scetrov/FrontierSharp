using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.Tests.HttpClient;

public class FakeComplexRequest : GetRequestModel<FakeComplexRequest> {
    public override string GetCacheKey() {
        return "FakeRequestCacheKey";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            {
                "key1", "value1"
            }, {
                "key2", "value2"
            }, {
                "key3", "value3"
            }
        };
    }

    public override string GetEndpoint() {
        return "/complex";
    }
}