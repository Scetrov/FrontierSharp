using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api.RequestModels;

public class OptimizeStargateNetworkPlacementRequestTests {
    [Fact]
    public void Should_HaveExpectedDefaultValues() {
        var request = new OptimizeStargateNetworkPlacementRequest();

        request.StartName.Should().Be("ICT-SVL");
        request.EndName.Should().Be("UB3-3QJ");
        request.MaxDistanceInLightYears.Should().Be(499m);
        request.NpcAvoidanceLevel.Should().Be(NpcAvoidanceLevel.High);
    }

    [Fact]
    public void GetCacheKey_ShouldReturnExpectedFormat() {
        var request = new OptimizeStargateNetworkPlacementRequest {
            StartName = "Alpha",
            EndName = "Beta",
            MaxDistanceInLightYears = 250.5m,
            NpcAvoidanceLevel = NpcAvoidanceLevel.Medium
        };

        var expected = "OptimizeStargateNetworkPlacementRequest_Alpha_Beta_250.5_Medium";

        request.GetCacheKey().Should().Be(expected);
    }

    [Fact]
    public void GetQueryParams_ShouldReturnCorrectDictionary() {
        var request = new OptimizeStargateNetworkPlacementRequest {
            StartName = "GateA",
            EndName = "GateB",
            MaxDistanceInLightYears = 123.45m,
            NpcAvoidanceLevel = NpcAvoidanceLevel.Off
        };

        var queryParams = request.GetQueryParams();

        queryParams.Should().ContainKey("start_name").WhoseValue.Should().Be("GateA");
        queryParams.Should().ContainKey("end_name").WhoseValue.Should().Be("GateB");
        queryParams.Should().ContainKey("max_distance").WhoseValue.Should().Be("123.45");
        queryParams.Should().ContainKey("npc_avoidance_level").WhoseValue.Should().Be("0");
    }
}