using AwesomeAssertions;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.WorldApi.RequestModel;
using Xunit;

namespace FrontierSharp.Tests.HttpClient;

public class RequestModelCacheKeyTests {
    [Fact]
    public void WorldApi_GetTypeById_ReturnsExpectedCacheKey() {
        var model = new GetTypeById { TypeId = 12345 };
        var key = model.GetCacheKey();
        string.IsNullOrWhiteSpace(key).Should().BeFalse();
        key.Should().Contain("WorldApi_Type_");
        key.Should().Contain("12345");
    }

    [Fact]
    public void WorldApi_GetListOfSolarSystems_IncludeTypeNameInCacheKey() {
        var model = new GetListOfSolarSystems { Limit = 100, Offset = 200 };
        var key = model.GetCacheKey();
        string.IsNullOrWhiteSpace(key).Should().BeFalse();
        key.Should().Contain("GetListOfSolarSystems");
        key.Should().Contain("Limit=100");
        key.Should().Contain("Offset=200");
    }

    [Fact]
    public void WorldApi_GetConfig_ReturnsStaticKey() {
        var model = new GetConfig();
        var key = model.GetCacheKey();
        key.Should().Be("WorldApi_Config");
    }

    [Fact]
    public void FrontierDevTools_CalculateDistance_ContainsTypeName() {
        var model = new CalculateDistanceRequest { SystemA = "SYS-A", SystemB = "SYS-B" };
        var key = model.GetCacheKey();
        string.IsNullOrWhiteSpace(key).Should().BeFalse();
        key.Should().Contain("CalculateDistanceRequest");
        key.Should().Contain("SYS-A");
        key.Should().Contain("SYS-B");
    }

    [Fact]
    public void FrontierDevTools_CalculateTravelDistance_ContainsTypeName() {
        // Use the actual properties (or defaults) for CalculateTravelDistanceRequest
        var model = new CalculateTravelDistanceRequest();
        var key = model.GetCacheKey();
        string.IsNullOrWhiteSpace(key).Should().BeFalse();
        key.Should().Contain("CalculateTravelDistanceRequest");
    }

    [Fact]
    public void FrontierDevTools_GetCharacterByName_ContainsTypeName() {
        var model = new GetCharacterByNameRequest { PlayerName = "bob" };
        var key = model.GetCacheKey();
        string.IsNullOrWhiteSpace(key).Should().BeFalse();
        key.Should().Contain("GetCharacterByNameRequest");
        key.Should().Contain("bob");
    }
}