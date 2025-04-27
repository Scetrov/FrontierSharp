using System.Text.Json;
using FluentAssertions;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using Xunit;

namespace FrontierSharp.Tests.FrontierDevTools.Api.Serialization;

public class OptimalStargateNetworkAndDeploymentResponseTests {
    [Fact]
    public async Task OptimalStargateNetworkAndDeploymentResponse_CanBeParsed() {
        var stream = ResourceHelper.GetEmbeddedResource(
            "FrontierSharp.Tests.FrontierDevTools.Api.Serialization.OptimalStargateNetworkAndDeploymentResponsePayload.json");
        var response = await JsonSerializer.DeserializeAsync<OptimalStargateNetworkAndDeploymentResponse>(stream);

        response.Should().NotBeNull();
        response.Results.Should().NotBeNull();
        response.Results.OptimalRoute.Should().NotBeNull();
        response.Results.FunctionalRoute.Should().NotBeNull();
        response.Results.TravelLog.Should().NotBeNull();
    }
}