namespace FrontierSharp.Mud.Linq.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class MudColumnAttribute(string columnName) : Attribute {
    public string ColumnName { get; init; } = columnName;
}