namespace MauiAsyncViewsDemo.Contracts;

public interface IAsyncInitializable
{
    bool IsInitialized { get; }

    Task InitializeAsync(CancellationToken cancellationToken);

    void CancelLoading();

    void Reset();
}
