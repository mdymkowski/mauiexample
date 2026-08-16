using CommunityToolkit.Mvvm.ComponentModel;
using MauiAsyncViewsDemo.Contracts;

namespace MauiAsyncViewsDemo.ViewModels;

public abstract partial class AsyncViewModelBase : ObservableObject, IAsyncInitializable
{
    private CancellationTokenSource? _loadCts;
    private Task? _initializeTask;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool hasError;

    public bool IsInitialized { get; private set; }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (IsInitialized)
            return Task.CompletedTask;

        // Chroni przed dwoma równoczesnymi InitializeAsync tego samego VM.
        if (_initializeTask is { IsCompleted: false })
            return _initializeTask;

        _initializeTask = InitializeCoreSafeAsync(cancellationToken);
        return _initializeTask;
    }

    private async Task InitializeCoreSafeAsync(CancellationToken externalToken)
    {
        CancelLoading();

        var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        _loadCts = cts;

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = null;

            await LoadAsync(cts.Token);

            cts.Token.ThrowIfCancellationRequested();
            IsInitialized = true;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // Wyjście z widoku / nowe ładowanie to normalna sytuacja.
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;

            // Czyścimy tylko wtedy, gdy nadal jest to ten sam CTS.
            if (ReferenceEquals(Interlocked.CompareExchange(ref _loadCts, null, cts), cts))
                cts.Dispose();
        }
    }

    protected abstract Task LoadAsync(CancellationToken cancellationToken);

    public virtual void CancelLoading()
    {
        var cts = Interlocked.Exchange(ref _loadCts, null);

        if (cts is null)
            return;

        try
        {
            cts.Cancel();
        }
        finally
        {
            cts.Dispose();
        }
    }

    public virtual void Reset()
    {
        CancelLoading();
        IsInitialized = false;
        HasError = false;
        ErrorMessage = null;
        _initializeTask = null;
    }
}
