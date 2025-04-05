using System.Net;
using Humanizer;
using NSubstitute;

namespace FrontierSharp.Tests.Utils.FakeHttpClientFactory;

public static class MockHttpClient {
    public static IHttpClientFactory CreateSimpleSubstitute(HttpStatusCode returnCode, HttpContent? content = null) {
        var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
        var responseMessage = new HttpResponseMessage(returnCode) {
            ReasonPhrase = returnCode.Humanize(),
            Content = content
        };
        var httpClient = new System.Net.Http.HttpClient(new FakeHttpMessageHandler(responseMessage));
        httpClientFactoryMock.CreateClient(Arg.Any<string>()).Returns(httpClient);
        return httpClientFactoryMock;
    }
}