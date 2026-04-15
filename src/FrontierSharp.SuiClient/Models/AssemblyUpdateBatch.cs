namespace FrontierSharp.SuiClient.Models;

public class AssemblyUpdateBatch {
    public bool IsInitialSnapshot { get; set; }
    public IReadOnlyList<Assembly> CurrentAssemblies { get; set; } = [];
    public IReadOnlyList<AssemblyChange> Changes { get; set; } = [];
}