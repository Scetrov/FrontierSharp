using System.Text.Json.Serialization;
using FrontierSharp.HttpClient.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class TravelDistanceResponse {
    [JsonPropertyName("max_travel_distance_ly")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal MaxTravelDistanceInLightYears { get; set; }
}