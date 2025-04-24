using System.Text;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;

namespace FrontierSharp.CommandLine.Utils;

public static class SpectreFrontierUtils {
    public static string FormatShipPath(this IEnumerable<ShipPathSegment> shipPath) {
        var sb = new StringBuilder();
        var shipPathSegments = shipPath as ShipPathSegment[] ?? shipPath.ToArray();
        sb.Append(shipPathSegments.First().From);
        foreach (var segment in shipPathSegments) {
            sb.Append(" \u2192 "); // Right arrow
            if (segment.JumpGate.HasValue) sb.Append("(g)");

            if (segment.Distance.HasValue) sb.Append($"({segment.Distance.Value}ly)");

            sb.Append($" {segment.To} ");
        }

        return sb.ToString().Trim();
    }
}