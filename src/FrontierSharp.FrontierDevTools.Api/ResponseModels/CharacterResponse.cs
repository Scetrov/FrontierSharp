using System.Numerics;
using System.Text.Json.Serialization;
using FrontierSharp.HttpClient.Serialization;

// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class CharacterResponse {
    [JsonPropertyName("address")] public string Address { get; set; } = string.Empty;

    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    [JsonPropertyName("corpId")]
    [JsonConverter(typeof(StringifiedInt32Converter))]
    public int CorpId { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("isSmartCharacter")]
    [JsonConverter(typeof(StringifiedBooleanConverter))]
    public bool IsSmartCharacter { get; set; }

    [JsonPropertyName("createdAt")]
    [JsonConverter(typeof(DateTimeOffsetConverter))]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("eveBalanceWei")]
    [JsonConverter(typeof(BigIntegerConverter))]
    public BigInteger EveBalanceWei { get; set; }

    [JsonPropertyName("gasBalanceWei")]
    [JsonConverter(typeof(BigIntegerConverter))]
    public BigInteger GasBalanceWei { get; set; }
}