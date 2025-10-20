using System.Text.RegularExpressions;
using AwesomeAssertions;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools;

public class SpectreFrontierUtilsTests {
    private static string Normalize(string s) {
        return Regex.Replace(s, "\\s+", " ").Trim();
    }

    [Fact]
    public void FormatShipPath_SingleSegment_NoGateNoDistance() {
        var segments = new List<ShipPathSegment> {
            new() { From = "A", To = "B", JumpGate = null, Distance = null }
        };

        var outStr = segments.FormatShipPath();
        Normalize(outStr).Should().Be("A → B");
        outStr.EndsWith(" ").Should().BeFalse(); // no trailing spaces
    }

    [Fact]
    public void FormatShipPath_SingleSegment_WithGateAndDistance() {
        var segments = new List<ShipPathSegment> {
            new() { From = "Alpha", To = "Beta", JumpGate = true, Distance = 10 }
        };

        var outStr = segments.FormatShipPath();
        Normalize(outStr).Should().Be("Alpha → (g)(10ly) Beta");
        outStr.Should().Contain("(g)");
        outStr.Should().Contain("(10ly)");
    }

    [Fact]
    public void FormatShipPath_MultipleSegments_Mixed() {
        var segments = new List<ShipPathSegment> {
            new() { From = "A", To = "B", JumpGate = true, Distance = 5 },
            new() { From = "B", To = "C", JumpGate = null, Distance = null }
        };

        var outStr = segments.FormatShipPath();
        Normalize(outStr).Should().Be("A → (g)(5ly) B → C");
        outStr.Should().Contain("C");
    }
}