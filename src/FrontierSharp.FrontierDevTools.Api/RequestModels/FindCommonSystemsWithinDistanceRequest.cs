using System.Globalization;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class FindCommonSystemsWithinDistanceRequest : GetRequestModel<FindCommonSystemsWithinDistanceRequest>, IGetRequestModel {
    public string SystemA { get; init; } = "E9H-LGK";
    public string SystemB { get; init; } = "IMK-85H";
    public decimal MaxDistance { get; init; } = 400m;

    public override string GetCacheKey() {
        return $"{nameof(FindCommonSystemsWithinDistanceRequest)}_{SystemA}_{SystemB}_{MaxDistance.ToString(CultureInfo.InvariantCulture)}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            { "system_a", SystemA },
            { "system_b", SystemB },
            { "max_distance", MaxDistance.ToString(CultureInfo.InvariantCulture) }
        };
    }

    public override string GetEndpoint() {
        return "/find_common_systems_within_distance";
    }
}