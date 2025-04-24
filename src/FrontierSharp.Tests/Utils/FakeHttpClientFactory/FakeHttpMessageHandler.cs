namespace FrontierSharp.Tests.Utils.FakeHttpClientFactory;

public class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken) {
        return Task.FromResult(response);
    }
}