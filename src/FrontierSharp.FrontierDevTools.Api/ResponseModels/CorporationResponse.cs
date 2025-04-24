using System.Text.Json.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class CorporationResponse {
    [JsonPropertyName("corp_characters")] public IEnumerable<string> CorpCharacters { get; set; } = [];
}