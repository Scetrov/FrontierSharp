namespace FrontierSharp.Mud.Linq.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class MudTableAttribute(string ns, string? tableName = null) : Attribute {
    public string Namespace { get; init; } = ns;
    public string? Name { get; init; } = tableName;
}

[AttributeUsage(AttributeTargets.Property)]
public class MudColumnAttribute(string columnName) : Attribute {
    public string ColumnName { get; init; } = columnName;
}