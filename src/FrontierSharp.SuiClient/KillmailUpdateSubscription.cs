namespace FrontierSharp.SuiClient;

internal sealed class KillmailUpdateSubscription : IKillmailUpdateSubscription {
    private readonly CancellationTokenSource _cancellationTokenSource;
    private int _disposed;

    public KillmailUpdateSubscription(CancellationTokenSource cancellationTokenSource, Task completion) {
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

