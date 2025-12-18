namespace FrontierSharp.HttpClient;

public class FrontierSharpHttpClientOptions {
    public string HttpClientName { get; set; } = string.Empty;
    public string BaseUri { get; set; } = string.Empty;
    // Optional JsonSerializerOptions that will be used when deserializing responses.
    // If not provided, a default options instance that enables reflection-based
    // serialization will be used so source-generator-free applications continue
    // to work in trimmed / AOT scenarios.
    public System.Text.Json.JsonSerializerOptions? JsonSerializerOptions { get; set; }
}