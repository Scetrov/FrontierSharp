namespace FrontierSharp.WorldApi;

public static class Helpers {
    public static string GenerateCacheKey(this IWorldApiEnumerableEndpoint endpoint) {
        return $"WorldApi_{endpoint.GetType().Name}_Limit={endpoint.Limit}_Offset={endpoint.Offset}";
    }

    public static Dictionary<string, string> GenerateParams(this IWorldApiEnumerableEndpoint endpoint) {
        return new Dictionary<string, string> {
            {
                "limit", endpoint.Limit.ToString()
            }, {
                "offset", endpoint.Offset.ToString()
            }
        };
    }
}