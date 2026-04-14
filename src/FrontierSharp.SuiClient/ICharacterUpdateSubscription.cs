namespace FrontierSharp.SuiClient;

public interface ICharacterUpdateSubscription : IDisposable {
    Task Completion { get; }
}