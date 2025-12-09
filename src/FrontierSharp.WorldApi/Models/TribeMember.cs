using System.Numerics;
using System.Text.Json.Serialization;
using FrontierSharp.HttpClient.Serialization;

namespace FrontierSharp.WorldApi.Models;

public class TribeMember {
    [JsonPropertyName("address")] public string Address { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    [JsonConverter(typeof(BigIntegerConverter))]
    public BigInteger Id { get; set; }
}