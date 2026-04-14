namespace FrontierSharp.SuiClient;

public interface IKillmailUpdateSubscription : IDisposable {
    Task Completion { get; }
}