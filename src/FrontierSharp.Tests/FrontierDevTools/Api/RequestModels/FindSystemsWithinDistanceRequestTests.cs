using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api.RequestModels;

public class FindSystemsWithinDistanceRequestTests {
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues() {
        var request = new FindSystemsWithinDistanceRequest();

        request.SystemName.Should().Be("EFN-12M");
        request.MaxDistance.Should().Be(60);
    }

    [Fact]
    public void GetCacheKey_ShouldReturnCorrectFormat() {
        var request = new FindSystemsWithinDistanceRequest {
            SystemName = "SYS-77",
            MaxDistance = 123.45m
        };

        var result = request.GetCacheKey();

        result.Should().Be("FindSystemsWithinDistanceRequest_SYS-77_123.45");
    }

    [Fact]
    public void GetQueryParams_ShouldContainCorrectKeysAndValues() {
        var request = new FindSystemsWithinDistanceRequest {
            SystemName = "HOME",
            MaxDistance = 42.75m
        };

        var queryParams = request.GetQueryParams();

        queryParams.Should().ContainKey("system_name").WhoseValue.Should().Be("HOME");
        queryParams.Should().ContainKey("max_distance").WhoseValue.Should().Be("42.75");
        queryParams["max_distance"].Should().Be("42.75");
    }

    [Fact]
    public void GetEndpoint_ShouldReturnExpectedEndpoint() {
        var request = new FindSystemsWithinDistanceRequest();

        var endpoint = request.GetEndpoint();

        endpoint.Should().Be("/find_systems_within_distance");
    }
}