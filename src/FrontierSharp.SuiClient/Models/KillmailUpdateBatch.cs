namespace FrontierSharp.SuiClient.Models;

public class KillmailUpdateBatch {
    public bool IsInitialSnapshot { get; set; }
    public IReadOnlyList<Killmail> CurrentKillmails { get; set; } = [];
    public IReadOnlyList<KillmailChange> Changes { get; set; } = [];
}

