using System.Collections.Generic;
using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api.RequestModels;

public class CalculateDistanceRequestTests {
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues() {
        var request = new CalculateDistanceRequest();

        request.SystemA.Should().Be("EFN-12M");
        request.SystemB.Should().Be("H.BQL.581");
    }

    [Fact]
    public void GetCacheKey_ShouldReturnCorrectFormat() {
        var request = new CalculateDistanceRequest {
            SystemA = "SYS-001",
            SystemB = "SYS-002"
        };

        var result = request.GetCacheKey();

        result.Should().Be("CalculateDistanceRequest_SYS-001_SYS-002");
    }

    [Fact]
    public void GetQueryParams_ShouldContainCorrectKeysAndValues() {
        var request = new CalculateDistanceRequest {
            SystemA = "SYS-100",
            SystemB = "SYS-200"
        };

        var queryParams = request.GetQueryParams();

        queryParams.Should().ContainKey("system_a").WhoseValue.Should().Be("SYS-100");
        queryParams.Should().ContainKey("system_b").WhoseValue.Should().Be("SYS-200");
        queryParams.Should().HaveCount(2);
    }

    [Fact]
    public void GetEndpoint_ShouldReturnCorrectEndpoint() {
        var request = new CalculateDistanceRequest();

        var endpoint = request.GetEndpoint();

        endpoint.Should().Be("/calculate_distance");
    }
}