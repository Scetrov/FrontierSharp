using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

public class TribeDetail : Tribe {
    [JsonPropertyName("members")]
    public IEnumerable<TribeMember> Members { get; set; } = [];
}
