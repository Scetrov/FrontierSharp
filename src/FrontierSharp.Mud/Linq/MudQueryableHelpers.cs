using System.Reflection;
using FrontierSharp.Mud.Linq.Attributes;

namespace FrontierSharp.Mud.Linq;

internal static class MudQueryableHelpers {
    internal static string ClassToTableName(Type entityType) {
        var attribute = entityType.GetCustomAttribute<MudTableAttribute>();

        if (attribute == null) {
            return $"\"{entityType.Name}\"";
        }

        var tableName = string.IsNullOrWhiteSpace(entityType.Name) ? attribute.Name : entityType.Name;

        return $"\"{attribute.Namespace}__{tableName}\"";
    }

    internal static string PropertyToColumnName(PropertyInfo? p) {
        if (p is null) {
            ArgumentNullException.ThrowIfNull(p);
        }

        return p.GetCustomAttribute<MudColumnAttribute>() is not { } columnAttribute ? p.Name : columnAttribute.ColumnName;
    }

    internal static string QuotedPropertyToColumnName(PropertyInfo? p) {
        return $"\"{PropertyToColumnName(p)}\"";
    }
}