using System.Globalization;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class CalculateTravelDistanceRequest : GetRequestModel<CalculateTravelDistanceRequest>, IGetRequestModel {
    public decimal CurrentFuel { get; init; } = 2_800.0m;
    public decimal FuelEfficiency { get; init; } = 80.00m;
    public decimal Mass { get; init; } = 4_795_000.0m;

    public override string GetCacheKey() {
        return $"{nameof(CalculateTravelDistanceRequest)}_{CurrentFuel}_{FuelEfficiency}_{Mass}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            { "current_fuel", CurrentFuel.ToString(CultureInfo.InvariantCulture) },
            { "fuel_efficiency", FuelEfficiency.ToString(CultureInfo.InvariantCulture) },
            { "mass", Mass.ToString(CultureInfo.InvariantCulture) }
        };
    }

    public override string GetEndpoint() {
        return "/calculate_travel_distance";
    }
}