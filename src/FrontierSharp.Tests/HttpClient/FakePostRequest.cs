using System.Net.Http.Json;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.Tests.HttpClient;

internal class FakePostRequest : PostRequestModel<FakePostRequest> {
    public override HttpContent GetHttpContent() {
        return JsonContent.Create(new { Name = "Test" });
    }

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