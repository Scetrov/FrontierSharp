using System.Text.Json.Serialization;
using FrontierSharp.FrontierDevTools.Api.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class TravelDistanceResponse {
    [JsonPropertyName("max_travel_distance_ly")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal MaxTravelDistanceInLightYears { get; set; }
}