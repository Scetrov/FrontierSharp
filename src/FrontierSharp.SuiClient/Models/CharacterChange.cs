namespace FrontierSharp.SuiClient.Models;

public class CharacterChange {
    public CharacterChangeType ChangeType { get; set; }
    public Character? Previous { get; set; }
    public Character? Current { get; set; }

    public TenantItemId Key => Current?.Key ?? Previous?.Key ?? new TenantItemId();
}