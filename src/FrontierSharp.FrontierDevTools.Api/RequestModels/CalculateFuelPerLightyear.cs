using System.Globalization;
using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class CalculateFuelPerLightyear : GetRequestModel<CalculateFuelPerLightyear>, IGetRequestModel {
    public decimal Mass { get; init; } = 4_795_000.0m;
    public decimal FuelEfficiency { get; init; } = 80.00m;

    public override string GetCacheKey() {
        return $"{nameof(CalculateFuelPerLightyear)}_{Mass}_{FuelEfficiency}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            {
                "mass", Mass.ToString(CultureInfo.InvariantCulture)
            }, {
                "fuel_efficiency", FuelEfficiency.ToString(CultureInfo.InvariantCulture)
            }
        };
    }

    public override string GetEndpoint() {
        return "/calculate_fuel_per_lightyear";
    }
}