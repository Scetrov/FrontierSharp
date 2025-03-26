using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using Xunit;

namespace FrontierSharp.FrontierDevTools.Tests.Api.RequestModels;

public class GetCharacterByNameRequestTests {
    [Fact]
    public void GetCacheKey_ShouldReturnCorrectCacheKey() {
        // Arrange
        const string playerName = "JohnDoe";
        var request = new GetCharacterByNameRequest { PlayerName = playerName };
        const string expected = "GetCharacterByNameRequest_JohnDoe";

        // Act
        var cacheKey = request.GetCacheKey();

        // Assert
        cacheKey.Should().Be(expected);
    }

    [Fact]
    public void GetQueryParams_ShouldReturnCorrectQueryParams() {
        // Arrange
        const string playerName = "JaneDoe";
        var request = new GetCharacterByNameRequest { PlayerName = playerName };
        var expected = new Dictionary<string, string> {
            { "player_name", playerName }
        };

        // Act
        var queryParams = request.GetQueryParams();

        // Assert
        queryParams.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetEndpoint_ShouldReturnCorrectEndpoint() {
        // Arrange
        var request = new GetCharacterByNameRequest();
        const string expected = "/get_character_by_name";

        // Act
        var endpoint = request.GetEndpoint();

        // Assert
        endpoint.Should().Be(expected);
    }
}