using System.Numerics;
using System.Text.Json.Serialization;
using FrontierSharp.FrontierDevTools.Api.Serialization;

namespace FrontierSharp.WorldApi.Models;

public class SmartCharacterDetail : SmartCharacter {
    [JsonPropertyName("tribeId")]
    public int TribeId { get; set; } = 0;

    [JsonPropertyName("eveBalanceInWei")]
    [JsonConverter(typeof(BigIntegerConverter))]
    public BigInteger EveBalanceInWei { get; set; } = 0;

    [JsonPropertyName("gasBalanceInWei")]
    [JsonConverter(typeof(BigIntegerConverter))]
    public BigInteger GasBalanceInWei { get; set; } = 0;

    [JsonPropertyName("smartAssemblies")]
    public IEnumerable<SmartAssembly> SmartAssemblies { get; set; } = [];

    [JsonPropertyName("portraitUrl")]
    public string PortraitUrl { get; set; } = string.Empty;
}