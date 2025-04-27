using System.Globalization;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class FindTravelRouteRequest : GetRequestModel<FindTravelRouteRequest>, IGetRequestModel {
    public string StartName { get; init; } = "ICT-SVL";
    public string EndName { get; init; } = "UB3-3QJ";

    public bool AvoidGates { get; init; }
    public decimal MaxDistanceInLightYears { get; init; } = 100m;

    public override string GetCacheKey() {
        return $"{nameof(FindTravelRouteRequest)}_{StartName}_{EndName}_{AvoidGates}_{MaxDistanceInLightYears}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            {
                "start_name", StartName
            }, {
                "end_name", EndName
            }, {
                "avoid_gates", AvoidGates.ToString()
            }, {
                "max_distance", MaxDistanceInLightYears.ToString(CultureInfo.InvariantCulture)
            }
        };
    }

    public override string GetEndpoint() {
        return "/find_travel_route";
    }
}