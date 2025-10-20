using AwesomeAssertions;
using FrontierSharp.WorldApi;
using Xunit;

namespace FrontierSharp.Tests.WorldApi;

public class HelpersTests {

    [Fact]
    public void GenerateCacheKey_IncludesTypeAndParams() {
        var endpoint = new DummyEndpoint { Limit = 100, Offset = 200 };
        var key = endpoint.GenerateCacheKey();
        key.Should().Be("WorldApi_DummyEndpoint_Limit=100_Offset=200");
    }

    [Fact]
    public void GenerateParams_ReturnsDictionaryWithStrings() {
        var endpoint = new DummyEndpoint { Limit = 5, Offset = 10 };
        var dict = endpoint.GenerateParams();
        dict.Should().NotBeNull();
        dict["limit"].Should().Be("5");
        dict["offset"].Should().Be("10");
        dict.Should().HaveCount(2);
    }

    private class DummyEndpoint : IWorldApiEnumerableEndpoint {
        public long Limit { get; set; }
        public long Offset { get; set; }
    }
}