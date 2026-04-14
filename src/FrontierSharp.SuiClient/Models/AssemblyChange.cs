namespace FrontierSharp.SuiClient.Models;

public class AssemblyChange {
    public AssemblyChangeType ChangeType { get; set; }
    public Assembly? Previous { get; set; }
    public Assembly? Current { get; set; }

    public TenantItemId Key => Current?.Key ?? Previous?.Key ?? new TenantItemId();
}