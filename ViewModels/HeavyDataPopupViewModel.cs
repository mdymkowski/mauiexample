using CommunityToolkit.Mvvm.ComponentModel;
using MauiAsyncViewsDemo.Models;
using MauiAsyncViewsDemo.Repositories;
using System.Collections.ObjectModel;

namespace MauiAsyncViewsDemo.ViewModels;

public partial class HeavyDataPopupViewModel : ObservableObject
{
    private readonly IHeavyDataRepository _repository;
    private CancellationTokenSource? _loadCts;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private ObservableCollection<HeavyDataItem> items = [];

    public HeavyDataPopupViewModel(IHeavyDataRepository repository)
    {
        _repository = repository;
    }

    public async Task LoadAsync(CancellationToken externalToken)
    {
        Cancel();

        var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        _loadCts = cts;

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            Items = [];

            var data = await _repository.GetLargeDataSetAsync(cts.Token);
            cts.Token.ThrowIfCancellationRequested();

            // Jedna podmiana kolekcji po zakończeniu pobierania.
            Items = new ObservableCollection<HeavyDataItem>(data);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;

            if (ReferenceEquals(Interlocked.CompareExchange(ref _loadCts, null, cts), cts))
                cts.Dispose();
        }
    }

    public void Cancel()
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
}
