using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class CalculateDistanceRequest : GetRequestModel<CalculateDistanceRequest> {
    public string SystemA { get; init; } = "EFN-12M";
    public string SystemB { get; init; } = "H.BQL.581";

    public override string GetCacheKey() {
        return $"{nameof(CalculateDistanceRequest)}_{SystemA}_{SystemB}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            { "system_a", SystemA },
            { "system_b", SystemB }
        };
    }

    public override string GetEndpoint() {
        return "/calculate_distance";
    }
}