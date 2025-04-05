using System.Globalization;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class OptimizeStargateNetworkPlacementRequest : GetRequestModel<OptimizeStargateNetworkPlacementRequest> {
    public string StartName { get; init; } = "ICT-SVL";
    public string EndName { get; init; } = "UB3-3QJ";
    public decimal MaxDistanceInLightYears { get; init; } = 499m;
    public NpcAvoidanceLevel NpcAvoidanceLevel { get; init; } = NpcAvoidanceLevel.High;

    public override string GetCacheKey() {
        return $"{nameof(OptimizeStargateNetworkPlacementRequest)}_{StartName}_{EndName}_{MaxDistanceInLightYears}_{NpcAvoidanceLevel}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            { "start_name", StartName },
            { "end_name", EndName },
            { "max_distance", MaxDistanceInLightYears.ToString(CultureInfo.InvariantCulture) },
            { "npc_avoidance_level", ((int)NpcAvoidanceLevel).ToString() }
        };
    }

    public override string GetEndpoint() {
        return "/optimize_stargate_network_placement";
    }
}