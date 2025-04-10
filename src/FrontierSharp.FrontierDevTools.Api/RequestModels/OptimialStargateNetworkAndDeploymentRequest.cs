using System.Globalization;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class OptimalStargateNetworkAndDeploymentRequest : GetRequestModel<OptimalStargateNetworkAndDeploymentRequest>, IGetRequestModel {
    public string StartName { get; init; } = "ICT-SVL";
    public string EndName { get; init; } = "UB3-3QJ";
    public decimal MaxStargateDistance { get; init; } = 499m;
    public NpcAvoidanceLevel NpcAvoidanceLevel { get; init; } = NpcAvoidanceLevel.High;
    public string IncludeShips { get; init; } = "Flegel";
    public bool AvoidGates { get; init; } = false;

    public override string GetCacheKey() {
        return $"{nameof(OptimalStargateNetworkAndDeploymentRequest)}_{StartName}_{EndName}_{MaxStargateDistance}_{NpcAvoidanceLevel}_{IncludeShips}_{AvoidGates}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            { "start_name", StartName },
            { "end_name", EndName },
            { "max_stargate_distance", MaxStargateDistance.ToString(CultureInfo.InvariantCulture) },
            { "npc_avoidance_level", ((int)NpcAvoidanceLevel).ToString() },
            { "include_ships", IncludeShips },
            { "avoid_gates", AvoidGates.ToString() }
        };
    }

    public override string GetEndpoint() {
        return "/optimal_stargate_network_and_deployment";
    }
}