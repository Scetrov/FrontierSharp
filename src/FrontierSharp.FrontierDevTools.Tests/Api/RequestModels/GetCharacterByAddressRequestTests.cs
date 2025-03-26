using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using Xunit;

namespace FrontierSharp.FrontierDevTools.Tests.Api.RequestModels;

public class GetCharacterByAddressRequestTests {
    [Fact]
    public void GetCacheKey_ShouldReturnCorrectCacheKey() {
        // Arrange
        const string address = "0x123ABC";
        var request = new GetCharacterByAddressRequest { Address = address };
        const string expected = "GetCharacterByAddressRequest_0x123ABC";

        // Act
        var cacheKey = request.GetCacheKey();

        // Assert
        cacheKey.Should().Be(expected);
    }

    [Fact]
    public void GetQueryParams_ShouldReturnCorrectQueryParams() {
        // Arrange
        const string address = "0xABCDEF";
        var request = new GetCharacterByAddressRequest { Address = address };
        var expected = new Dictionary<string, string> {
            { "player_address", address }
        };

        // Act
        var queryParams = request.GetQueryParams();

        // Assert
        queryParams.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetEndpoint_ShouldReturnCorrectEndpoint() {
        // Arrange
        var request = new GetCharacterByAddressRequest();
        const string expected = "/get_character_by_address";

        // Act
        var endpoint = request.GetEndpoint();

        // Assert
        endpoint.Should().Be(expected);
    }
}