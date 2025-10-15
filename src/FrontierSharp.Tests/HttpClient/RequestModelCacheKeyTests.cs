using Xunit;
using FrontierSharp.WorldApi.RequestModel;
using FrontierSharp.FrontierDevTools.Api.RequestModels;

namespace FrontierSharp.Tests.HttpClient
{
    public class RequestModelCacheKeyTests
    {
        [Fact]
        public void WorldApi_GetTypeById_ReturnsExpectedCacheKey()
        {
            var model = new GetTypeById { TypeId = 12345 };
            var key = model.GetCacheKey();
            Assert.False(string.IsNullOrWhiteSpace(key));
            Assert.Contains("WorldApi_Type_", key);
            Assert.Contains("12345", key);
        }

        [Fact]
        public void WorldApi_GetListOfSolarSystems_IncludeTypeNameInCacheKey()
        {
            var model = new GetListOfSolarSystems { Limit = 100, Offset = 200 };
            var key = model.GetCacheKey();
            Assert.False(string.IsNullOrWhiteSpace(key));
            Assert.Contains("GetListOfSolarSystems", key);
            Assert.Contains("Limit=100", key);
            Assert.Contains("Offset=200", key);
        }

        [Fact]
        public void WorldApi_GetConfig_ReturnsStaticKey()
        {
            var model = new GetConfig();
            var key = model.GetCacheKey();
            Assert.Equal("WorldApi_Config", key);
        }

        [Fact]
        public void FrontierDevTools_CalculateDistance_ContainsTypeName()
        {
            var model = new CalculateDistanceRequest { SystemA = "SYS-A", SystemB = "SYS-B" };
            var key = model.GetCacheKey();
            Assert.False(string.IsNullOrWhiteSpace(key));
            Assert.Contains("CalculateDistanceRequest", key);
            Assert.Contains("SYS-A", key);
            Assert.Contains("SYS-B", key);
        }

        [Fact]
        public void FrontierDevTools_CalculateTravelDistance_ContainsTypeName()
        {
            // Use the actual properties (or defaults) for CalculateTravelDistanceRequest
            var model = new CalculateTravelDistanceRequest();
            var key = model.GetCacheKey();
            Assert.False(string.IsNullOrWhiteSpace(key));
            Assert.Contains("CalculateTravelDistanceRequest", key);
        }

        [Fact]
        public void FrontierDevTools_GetCharacterByName_ContainsTypeName()
        {
            var model = new GetCharacterByNameRequest { PlayerName = "bob" };
            var key = model.GetCacheKey();
            Assert.False(string.IsNullOrWhiteSpace(key));
            Assert.Contains("GetCharacterByNameRequest", key);
            Assert.Contains("bob", key);
        }
    }
}
