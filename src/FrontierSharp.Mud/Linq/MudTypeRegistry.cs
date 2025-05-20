namespace FrontierSharp.Mud.Linq;

public class MudTypeRegistry {
    private readonly Dictionary<Type, object> _typeRegistry = new();

    public void AddType<T>(IMudTypeConverter<T> typeConverter) {
        _typeRegistry.Add(typeof(T), typeConverter);
    }

    public IMudTypeConverter<T> GetTypeConverter<T>() {
        if (!_typeRegistry.TryGetValue(typeof(T), out var converter)) {
            return new GenericMudTypeConverter<T>();
        }
        
        if (converter is IMudTypeConverter<T> type) {
            return type;
        }

        return new GenericMudTypeConverter<T>();
    }
}