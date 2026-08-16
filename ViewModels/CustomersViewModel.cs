using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiAsyncViewsDemo.Models;
using MauiAsyncViewsDemo.Repositories;
using System.Collections.ObjectModel;

namespace MauiAsyncViewsDemo.ViewModels;

public partial class CustomersViewModel : AsyncViewModelBase
{
    private const int PageSize = 50;

    private readonly ICustomerRepository _repository;

    private int _nextSkip;
    private bool _hasMore = true;
    private int _loadMoreGate;
    private CancellationTokenSource? _loadMoreCts;

    [ObservableProperty]
    private ObservableCollection<CustomerModel> customers = [];

    [ObservableProperty]
    private bool isLoadingMore;

    public CustomersViewModel(ICustomerRepository repository)
    {
        _repository = repository;
    }

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        var firstPage = await _repository.GetPageAsync(
            skip: 0,
            take: PageSize,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        Customers = new ObservableCollection<CustomerModel>(firstPage);
        _nextSkip = firstPage.Count;
        _hasMore = firstPage.Count == PageSize;
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (!_hasMore || !IsInitialized)
            return;

        // RemainingItemsThreshold może odpalić się kilka razy szybko pod rząd.
        if (Interlocked.Exchange(ref _loadMoreGate, 1) == 1)
            return;

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        _loadMoreCts = cts;

        try
        {
            IsLoadingMore = true;

            var page = await _repository.GetPageAsync(
                _nextSkip,
                PageSize,
                cts.Token);

            cts.Token.ThrowIfCancellationRequested();

            // Dodajemy tylko małą paczkę, nie tysiące rekordów naraz.
            foreach (var customer in page)
                Customers.Add(customer);

            _nextSkip += page.Count;
            _hasMore = page.Count == PageSize;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
        }
        finally
        {
            IsLoadingMore = false;

            if (ReferenceEquals(Interlocked.CompareExchange(ref _loadMoreCts, null, cts), cts))
                cts.Dispose();

            Volatile.Write(ref _loadMoreGate, 0);
        }
    }

    public override void CancelLoading()
    {
        base.CancelLoading();

        var cts = Interlocked.Exchange(ref _loadMoreCts, null);

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

    public override void Reset()
    {
        base.Reset();

        Customers = [];
        _nextSkip = 0;
        _hasMore = true;
    }
}
