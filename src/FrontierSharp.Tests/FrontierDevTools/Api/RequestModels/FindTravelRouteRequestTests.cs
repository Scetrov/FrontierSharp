using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api.RequestModels;

public class FindTravelRouteRequestTests {
    [Fact]
    public void Should_HaveExpectedDefaultValues() {
        var request = new FindTravelRouteRequest();

        request.StartName.Should().Be("ICT-SVL");
        request.EndName.Should().Be("UB3-3QJ");
        request.AvoidGates.Should().BeFalse();
        request.MaxDistanceInLightYears.Should().Be(100m);
    }

    [Fact]
    public void GetCacheKey_ShouldReturnExpectedFormat() {
        var request = new FindTravelRouteRequest {
            StartName = "Alpha",
            EndName = "Beta",
            AvoidGates = true,
            MaxDistanceInLightYears = 250.5m
        };

        var expected = "FindTravelRouteRequest_Alpha_Beta_True_250.5";

        request.GetCacheKey().Should().Be(expected);
    }

    [Fact]
    public void GetQueryParams_ShouldReturnExpectedDictionary() {
        var request = new FindTravelRouteRequest {
            StartName = "Sol-A",
            EndName = "Sol-B",
            AvoidGates = true,
            MaxDistanceInLightYears = 123.45m
        };

        var query = request.GetQueryParams();

        query.Should().ContainKey("start_name").WhoseValue.Should().Be("Sol-A");
        query.Should().ContainKey("end_name").WhoseValue.Should().Be("Sol-B");
        query.Should().ContainKey("avoid_gates").WhoseValue.Should().Be("True");
        query.Should().ContainKey("max_distance").WhoseValue.Should().Be("123.45");
    }

    [Fact]
    public void GetEndpoint_ShouldReturnExpectedPath() {
        var request = new FindTravelRouteRequest();

        request.GetEndpoint().Should().Be("/find_travel_route");
    }
}