using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

public class Manufacturing {
    [JsonPropertyName("isParentNodeOnline")]
    public bool IsParentNodeOnline { get; set; } = false;
}