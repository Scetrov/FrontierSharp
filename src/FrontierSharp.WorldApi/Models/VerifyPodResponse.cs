// filepath: c:\source\FrontierSharp\src\FrontierSharp.WorldApi\Models\VerifyPodResponse.cs
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

public class VerifyPodResponse {
    [JsonPropertyName("error")] public string Error { get; set; } = string.Empty;
    [JsonPropertyName("result")] public bool Result { get; set; }
}

