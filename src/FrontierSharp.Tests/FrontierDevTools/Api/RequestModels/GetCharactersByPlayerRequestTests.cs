using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FluentAssertions;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api.RequestModels;

public class GetCharactersByPlayerRequestTests
{
    [Fact]
    public void GetEndpoint_ShouldReturnCorrectEndpoint()
    {
        // Arrange
        var request = new GetCharactersByPlayerRequest();

        // Act
        var endpoint = request.GetEndpoint();

        // Assert
        endpoint.Should().Be("/get_chars_by_player");
    }

    [Fact]
    public void GetCacheKey_ShouldIncludeClassNameAndPlayerName()
    {
        // Arrange
        var request = new GetCharactersByPlayerRequest { PlayerName = "TestPlayer" };

        // Act
        var cacheKey = request.GetCacheKey();

        // Assert
        cacheKey.Should().Be("GetCharactersByPlayerRequest_TestPlayer");
    }

    [Fact]
    public void GetQueryParams_ShouldReturnPlayerNameParameter()
    {
        // Arrange
        var request = new GetCharactersByPlayerRequest { PlayerName = "Alice" };

        // Act
        var queryParams = request.GetQueryParams();

        // Assert
        queryParams.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new KeyValuePair<string, string>("player_name", "Alice"));
    }

    [Fact]
    public void DefaultConstructor_ShouldInitializePlayerNameAsEmptyString()
    {
        // Act
        var request = new GetCharactersByPlayerRequest();

        // Assert
        request.PlayerName.Should().BeEmpty();
    }
}