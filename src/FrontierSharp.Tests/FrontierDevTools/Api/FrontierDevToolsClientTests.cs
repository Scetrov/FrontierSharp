using FluentAssertions;
using FluentResults;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using FrontierSharp.HttpClient;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api;

public class FrontierDevToolsClientTests {
    private readonly IFrontierSharpHttpClient _httpClient = Substitute.For<IFrontierSharpHttpClient>();
    private readonly FrontierDevToolsClient _sut;

    public FrontierDevToolsClientTests() {
        _sut = new FrontierDevToolsClient(_httpClient);
    }

    [Fact]
    public async Task GetCharactersByName_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CharactersResponse>>();
        var name = "JaneDoe";

        _httpClient.Get<GetCharacterByNameRequest, CharactersResponse>(
            Arg.Is<GetCharacterByNameRequest>(r => r.PlayerName == name),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetCharactersByName(name);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetCharactersByAddress_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CharactersResponse>>();
        var address = "0xabc123";

        _httpClient.Get<GetCharacterByAddressRequest, CharactersResponse>(
            Arg.Is<GetCharacterByAddressRequest>(r => r.Address == address),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetCharactersByAddress(address);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetCharactersByCorpId_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CorporationResponse>>();
        var corpId = 99;

        _httpClient.Get<GetCharactersByCorpIdRequest, CorporationResponse>(
            Arg.Is<GetCharactersByCorpIdRequest>(r => r.CorpId == corpId),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetCharactersByCorpId(corpId);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetCharactersByPlayer_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<CorporationResponse>>();
        var playerName = "CommanderShepard";

        _httpClient.Get<GetCharactersByPlayerRequest, CorporationResponse>(
            Arg.Is<GetCharactersByPlayerRequest>(r => r.PlayerName == playerName),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetCharactersByPlayer(playerName);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetGateNetwork_ShouldCallHttpClientWithCorrectRequest() {
        var expected = Substitute.For<IResult<GateNetworkResponse>>();
        var identifier = "Sol";

        _httpClient.Get<GetGateNetworkRequest, GateNetworkResponse>(
            Arg.Is<GetGateNetworkRequest>(r => r.Identifier == identifier),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.GetGateNetwork(identifier);

        result.Should().Be(expected);
    }
}