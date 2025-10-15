// ...existing code...
using Xunit;
using FrontierSharp.WorldApi;
using System.Collections.Generic;

namespace FrontierSharp.Tests.WorldApi
{
    public class HelpersTests
    {
        private class DummyEndpoint : IWorldApiEnumerableEndpoint
        {
            public long Limit { get; set; }
            public long Offset { get; set; }
        }

        [Fact]
        public void GenerateCacheKey_IncludesTypeAndParams()
        {
            var endpoint = new DummyEndpoint { Limit = 100, Offset = 200 };
            var key = endpoint.GenerateCacheKey();
            Assert.Equal("WorldApi_DummyEndpoint_Limit=100_Offset=200", key);
        }

        [Fact]
        public void GenerateParams_ReturnsDictionaryWithStrings()
        {
            var endpoint = new DummyEndpoint { Limit = 5, Offset = 10 };
            var dict = endpoint.GenerateParams();
            Assert.NotNull(dict);
            Assert.Equal("5", dict["limit"]);
            Assert.Equal("10", dict["offset"]);
            Assert.Equal(2, dict.Count);
        }
    }
}
// ...existing code...
