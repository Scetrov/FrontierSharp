using System.Net;
using System.Text;

namespace FrontierSharp.Tests;

public class SubstitutableHttpClientFactory(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : IHttpClientFactory {
    public System.Net.Http.HttpClient CreateClient(string name) {
        var handler1 = new DelegatingHandlerStub(handler);
        return new System.Net.Http.HttpClient(handler1) {
            BaseAddress = new Uri("https://test.local") // optional
        };
    }

    public static IHttpClientFactory CreateWithPayload(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK, Encoding? encoding = null, string mediaType = "application/json") {
        var factory = new SubstitutableHttpClientFactory((_, _) =>
            Task.FromResult(new HttpResponseMessage {
                Content = new StringContent(responseBody, encoding ?? Encoding.UTF8, mediaType),
                StatusCode = statusCode
            }));

        return factory;
    }

    public static IHttpClientFactory CreateInternalServerError() {
        var factory = new SubstitutableHttpClientFactory((_, _) =>
            Task.FromResult(new HttpResponseMessage {
                Content = new StringContent("Internal Server Error", Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.InternalServerError
            }));

        return factory;
    }

    private class DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc) : HttpMessageHandler {

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            return handlerFunc(request, cancellationToken);
        }
    }
}