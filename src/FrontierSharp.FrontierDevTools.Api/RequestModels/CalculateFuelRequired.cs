using System.Globalization;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class CalculateFuelRequired : GetRequestModel<CalculateFuelRequired>, IGetRequestModel {
    public decimal Mass { get; init; } = 4_795_000.0m;
    public decimal Lightyears { get; init; } = 420.0m;
    public decimal FuelEfficiency { get; init; } = 80.00m;
    public override string GetCacheKey() {
        return $"{nameof(CalculateFuelRequired)}_{Mass}_{Lightyears}_{FuelEfficiency}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            { "mass", Mass.ToString(CultureInfo.InvariantCulture) },
            { "lightyears", Lightyears.ToString(CultureInfo.InvariantCulture) },
            { "fuel_efficiency", FuelEfficiency.ToString(CultureInfo.InvariantCulture) },
        };
    }

    public override string GetEndpoint() {
        return "/calculate_fuel_required";
    }
}