using System.Globalization;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class FindSystemsWithingDistanceRequest : GetRequestModel<FindSystemsWithingDistanceRequest> {
    public string SystemName { get; init; } = "EFN-12M";
    public decimal MaxDistance { get; init; } = 60;

    public override string GetCacheKey() {
        return $"{nameof(CalculateDistanceRequest)}_{SystemName}_{MaxDistance}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            { "system_name", SystemName },
            { "max_distance", MaxDistance.ToString(CultureInfo.InvariantCulture) }
        };
    }

    public override string GetEndpoint() {
        return "/find_systems_within_distance";
    }
}