using System;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

public class Tribe {
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("nameShort")] public string NameShort { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("memberCount")] public int MemberCount { get; set; }
    [JsonPropertyName("taxRate")] public double TaxRate { get; set; }
    [JsonPropertyName("tribeUrl")] public string TribeUrl { get; set; } = string.Empty;
    [JsonPropertyName("foundedAt")] public DateTimeOffset FoundedAt { get; set; }
}
