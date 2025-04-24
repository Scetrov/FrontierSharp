using System.Text.Json.Serialization;
using FrontierSharp.FrontierDevTools.Api.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class RouteResults {
    [JsonPropertyName("optimal_route")] public required OptimalRoute OptimalRoute { get; set; }

    [JsonPropertyName("functional_route")]
    public required IEnumerable<FunctionalRouteSegment> FunctionalRoute { get; set; }

    [JsonPropertyName("travel_log")] public required IEnumerable<TravelLogEntry> TravelLog { get; set; }

    [JsonPropertyName("over_deployed")]
    [JsonConverter(typeof(StringifiedBooleanConverter))]
    public bool OverDeployed { get; set; }
}