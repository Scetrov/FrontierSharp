using System.Globalization;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class FindSystemsWithinDistanceRequest : GetRequestModel<FindSystemsWithinDistanceRequest> {
    public string SystemName { get; init; } = "EFN-12M";
    public decimal MaxDistance { get; init; } = 60;

    public override string GetCacheKey() =>
        $"{nameof(FindSystemsWithinDistanceRequest)}_{SystemName}_{MaxDistance}";

    public override Dictionary<string, string> GetQueryParams() =>
        new() {
            { "system_name", SystemName },
            { "max_distance", MaxDistance.ToString(CultureInfo.InvariantCulture) }
        };

    public override string GetEndpoint() =>
        "/find_systems_within_distance";
}