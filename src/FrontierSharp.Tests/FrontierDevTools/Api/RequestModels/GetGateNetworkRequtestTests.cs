using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api.RequestModels;

public class GetGateNetworkRequestTests {
    [Fact]
    public void GetCacheKey_ShouldIncludeClassNameAndIdentifier() {
        // Arrange
        var request = new GetGateNetworkRequest { Identifier = "gate-123" };

        // Act
        var cacheKey = request.GetCacheKey();

        // Assert
        cacheKey.Should().Be("GetGateNetworkRequest_gate-123");
    }

    [Fact]
    public void GetQueryParams_ShouldReturnDictionaryWithIdentifier() {
        // Arrange
        var request = new GetGateNetworkRequest { Identifier = "gate-456" };

        // Act
        var queryParams = request.GetQueryParams();

        // Assert
        queryParams.Should().ContainKey("identifier").WhoseValue.Should().Be("gate-456");
        queryParams.Should().HaveCount(1);
    }

    [Fact]
    public void GetEndpoint_ShouldReturnCorrectApiPath() {
        // Arrange
        var request = new GetGateNetworkRequest();

        // Act
        var endpoint = request.GetEndpoint();

        // Assert
        endpoint.Should().Be("/get_gate_network");
    }
}