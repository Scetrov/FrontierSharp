namespace FrontierSharp.SuiClient.Models;

public class KillmailChange {
    public KillmailChangeType ChangeType { get; set; }
    public Killmail? Previous { get; set; }
    public Killmail? Current { get; set; }

    public string Id => Current?.Id ?? Previous?.Id ?? string.Empty;
}