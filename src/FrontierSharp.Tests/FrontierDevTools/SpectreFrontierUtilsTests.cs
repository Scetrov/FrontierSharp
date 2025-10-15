using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;

namespace FrontierSharp.Tests.FrontierDevTools
{
    public class SpectreFrontierUtilsTests
    {
        private static string Normalize(string s) => Regex.Replace(s, "\\s+", " ").Trim();

        [Fact]
        public void FormatShipPath_SingleSegment_NoGateNoDistance()
        {
            var segments = new List<ShipPathSegment> {
                new ShipPathSegment { From = "A", To = "B", JumpGate = null, Distance = null }
            };

            var outStr = SpectreFrontierUtils.FormatShipPath(segments);
            Assert.Equal("A → B", Normalize(outStr));
            Assert.False(outStr.EndsWith(" ")); // no trailing spaces
        }

        [Fact]
        public void FormatShipPath_SingleSegment_WithGateAndDistance()
        {
            var segments = new List<ShipPathSegment> {
                new ShipPathSegment { From = "Alpha", To = "Beta", JumpGate = true, Distance = 10 }
            };

            var outStr = SpectreFrontierUtils.FormatShipPath(segments);
            Assert.Equal("Alpha → (g)(10ly) Beta", Normalize(outStr));
            Assert.Contains("(g)", outStr);
            Assert.Contains("(10ly)", outStr);
        }

        [Fact]
        public void FormatShipPath_MultipleSegments_Mixed()
        {
            var segments = new List<ShipPathSegment> {
                new ShipPathSegment { From = "A", To = "B", JumpGate = true, Distance = 5 },
                new ShipPathSegment { From = "B", To = "C", JumpGate = null, Distance = null }
            };

            var outStr = SpectreFrontierUtils.FormatShipPath(segments);
            Assert.Equal("A → (g)(5ly) B → C", Normalize(outStr));
            Assert.Contains("C", outStr);
        }
    }
}
