namespace FrontierSharp.Mud.Linq;

public class GenericMudTypeConverter<T> : MudTypeConverterBase<T>, IMudTypeConverter<T> {
    public T CreateInstance(object[] row) {
        var instance = Activator.CreateInstance<T>();

        foreach (var prop in typeof(T).GetProperties()) {
            var header = MudQueryableHelpers.PropertyToColumnName(prop);
            if (!HeaderMap.TryGetValue(header, out var index)) {
                throw new InvalidOperationException($"Failed to find the correct index for '{header}' on '{typeof(T).Name}.{prop.Name}'.");   
            }
            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            var value = Convert.ChangeType(row[index], targetType);
            prop.SetValue(instance, value);
        }

        return instance;
    }
}