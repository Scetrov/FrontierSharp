using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

[ExcludeFromCodeCoverage]
public class GameType {
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

    [JsonPropertyName("mass")] public double Mass { get; set; }

    [JsonPropertyName("radius")] public double Radius { get; set; }

    [JsonPropertyName("volume")] public double Volume { get; set; }

    [JsonPropertyName("portionSize")] public int PortionSize { get; set; }

    [JsonPropertyName("groupName")] public string GroupName { get; set; } = string.Empty;

    [JsonPropertyName("groupId")] public int GroupId { get; set; }

    [JsonPropertyName("categoryName")] public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("categoryId")] public int CategoryId { get; set; }

    [JsonPropertyName("iconUrl")] public string IconUrl { get; set; } = string.Empty;
}