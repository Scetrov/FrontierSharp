using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

[ExcludeFromCodeCoverage]
public class Manufacturing {
    [JsonPropertyName("isParentNodeOnline")]
    public bool IsParentNodeOnline { get; set; } = false;
}