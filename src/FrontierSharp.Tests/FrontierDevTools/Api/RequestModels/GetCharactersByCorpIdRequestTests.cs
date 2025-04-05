using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using Microsoft.CSharp.RuntimeBinder;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api.RequestModels;

public class GetCharactersByCorpIdRequestTests {
    [Fact]
    public void CorpId_Should_Default_To_Zero() {
        var request = new GetCharactersByCorpIdRequest();

        request.CorpId.Should().Be(0);
    }

    [Theory]
    [InlineData(0, "/get_chars_by_corp_id")]
    [InlineData(42, "/get_chars_by_corp_id")]
    [InlineData(int.MaxValue, "/get_chars_by_corp_id")]
    public void GetEndpoint_Should_Return_Expected_Value(int corpId, string expectedEndpoint) {
        var request = new GetCharactersByCorpIdRequest { CorpId = corpId };

        var endpoint = request.GetEndpoint();

        endpoint.Should().Be(expectedEndpoint);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(123)]
    [InlineData(int.MaxValue)]
    public void GetCacheKey_Should_Include_CorpId(int corpId) {
        var request = new GetCharactersByCorpIdRequest { CorpId = corpId };

        var cacheKey = request.GetCacheKey();

        cacheKey.Should().Be($"GetCharactersByCorpIdRequest_{corpId}");
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(999, "999")]
    [InlineData(int.MaxValue, "2147483647")]
    public void GetQueryParams_Should_Return_Correct_Dictionary(int corpId, string expectedValue) {
        var request = new GetCharactersByCorpIdRequest { CorpId = corpId };

        var queryParams = request.GetQueryParams();

        queryParams.Should().ContainKey("corp_id");
        queryParams["corp_id"].Should().Be(expectedValue);
    }

    [Fact]
    public void GetQueryParams_Should_Return_New_Instance_Each_Time() {
        var request = new GetCharactersByCorpIdRequest { CorpId = 101 };

        var first = request.GetQueryParams();
        var second = request.GetQueryParams();

        first.Should().NotBeSameAs(second);
        first.Should().BeEquivalentTo(second);
    }

    [Fact]
    public void CacheKey_Should_Be_Deterministic_For_Same_Input() {
        var request1 = new GetCharactersByCorpIdRequest { CorpId = 55 };
        var request2 = new GetCharactersByCorpIdRequest { CorpId = 55 };

        request1.GetCacheKey().Should().Be(request2.GetCacheKey());
    }

    [Fact]
    public void Model_Should_Be_Immutable() {
        var request = new GetCharactersByCorpIdRequest { CorpId = 77 };

        Action act = () => ((dynamic)request).CorpId = 88;

        act.Should().Throw<RuntimeBinderException>();
    }
}