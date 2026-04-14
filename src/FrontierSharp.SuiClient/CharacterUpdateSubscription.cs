namespace FrontierSharp.SuiClient;

internal sealed class CharacterUpdateSubscription : ICharacterUpdateSubscription {
    private readonly CancellationTokenSource _cancellationTokenSource;
    private int _disposed;

    public CharacterUpdateSubscription(CancellationTokenSource cancellationTokenSource, Task completion) {
        _cancellationTokenSource = cancellationTokenSource;
        Completion = completion.ContinueWith(task => {
            _cancellationTokenSource.Dispose();
            return task;
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).Unwrap();
    }

    public Task Completion { get; }

    public void Dispose() {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        _cancellationTokenSource.Cancel();
    }
}