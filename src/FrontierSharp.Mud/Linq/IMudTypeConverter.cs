using System.Text.Json.Nodes;

namespace FrontierSharp.Mud.Linq;

public interface IMudTypeConverter<out T> {
    IMudTypeConverter<T> WithHeadings(JsonNode?[] data);
    T CreateInstance(object[] row);
}