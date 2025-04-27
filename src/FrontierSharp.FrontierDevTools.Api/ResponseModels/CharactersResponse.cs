using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class CharactersResponse {
    [JsonPropertyName("characters")] public IEnumerable<CharacterResponse> Characters { get; set; } = [];
}