namespace FrontierSharp.SuiClient;

public interface IAssemblyUpdateSubscription : IDisposable {
    Task Completion { get; }
}

