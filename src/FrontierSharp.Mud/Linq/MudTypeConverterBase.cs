using System.Text.Json.Nodes;

namespace FrontierSharp.Mud.Linq;

public abstract class MudTypeConverterBase<T> {
    protected Dictionary<string, int> HeaderMap = new();

    public IMudTypeConverter<T> WithHeadings(JsonNode?[] data) {
        HeaderMap = MapHeadings(data);
        return (IMudTypeConverter<T>)this;
    }

    private static Dictionary<string, int> MapHeadings(JsonNode?[]? data) {
        var headers = data?.First()?.AsArray();

        if (headers is null)
            throw new InvalidOperationException("Unable to parse the headers, unexpected payload.");
            
        var headerMap = headers
            .Select((h, index) => new { Key = h!.GetValue<string>(), Index = index })
            .ToDictionary(x => x.Key, x => x.Index);
        
        return headerMap;
    }
}