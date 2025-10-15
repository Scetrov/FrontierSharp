using System.Text;
using System.Text.Json;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class PostVerifyPod : PostRequestModel<PostVerifyPod> {
    public object? PodData { get; set; }

    public override HttpContent GetHttpContent() {
        var json = JsonSerializer.Serialize(PodData);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string>();
    }

    public override string GetEndpoint() => "/v2/pod/verify";
}
