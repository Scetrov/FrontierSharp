namespace FrontierSharp.SuiClient.Models;

public class CharacterSubscriptionOptions {
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);
    public int PageSize { get; set; } = 50;
    public bool EmitInitialSnapshot { get; set; }
}