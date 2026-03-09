using AwesomeAssertions;
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
}