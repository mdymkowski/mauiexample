using CommunityToolkit.Maui.Markup;
using MauiAsyncViewsDemo.Contracts;

namespace MauiAsyncViewsDemo.Views;

public sealed class DashboardPage : ContentPage
{
    private readonly IServiceProvider _services;

    // Viewy są tworzone raz dla życia tej strony.
    private readonly AppointmentsView _appointmentsView;
    private readonly TasksView _tasksView;
    private readonly CustomersView _customersView;

    private CancellationTokenSource? _pageCts;
    private bool _initializationStarted;

    public DashboardPage(IServiceProvider services)
    {
        _services = services;

        Title = "Async Views Demo";

        // To odpowiada Twojemu scenariuszowi.
        // GetRequiredService tworzy View + jego ViewModel,
        // ale NIE uruchamia LoadData.
        _appointmentsView = _services.GetRequiredService<AppointmentsView>();
        _tasksView = _services.GetRequiredService<TasksView>();
        _customersView = _services.GetRequiredService<CustomersView>();

        Content = BuildLayout();
    }

    private View BuildLayout()
    {
        var reloadButton = new Button
        {
            Text = "Reset + załaduj ponownie",
            HorizontalOptions = LayoutOptions.End
        };

        reloadButton.Clicked += async (_, _) =>
        {
            ResetAllChildren();
            await StartLoadingAsync(force: true);
        };

        return new Grid
        {
            Padding = 18,

            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(new GridLength(1, GridUnitType.Star))
            },

            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star))
            },

            RowSpacing = 16,
            ColumnSpacing = 16,

            Children =
            {
                new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto)
                    },
                    Children =
                    {
                        new VerticalStackLayout
                        {
                            Children =
                            {
                                new Label
                                {
                                    Text = "Dashboard",
                                    FontSize = 30,
                                    FontAttributes = FontAttributes.Bold
                                },
                                new Label
                                {
                                    Text = "Child views mają własne ViewModele. Konstruktor buduje UI, dane startują dopiero po pojawieniu się strony."
                                }
                            }
                        },
                        reloadButton.Column(1)
                    }
                }
                .ColumnSpan(2),

                _appointmentsView.Row(1).Column(0),
                _tasksView.Row(1).Column(1),

                new Border
                {
                    StrokeThickness = 1,
                    Padding = 10,
                    Content = _customersView
                }
                .Row(2)
                .ColumnSpan(2)
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_initializationStarted)
            return;

        _initializationStarted = true;

        // Dajemy MAUI możliwość najpierw wyrenderować Page.
        await Task.Yield();

        await StartLoadingAsync(force: false);
    }

    protected override void OnDisappearing()
    {
        CancelAllLoading();
        base.OnDisappearing();
    }

    private async Task StartLoadingAsync(bool force)
    {
        CancelAllLoading();

        _pageCts = new CancellationTokenSource();
        var ct = _pageCts.Token;

        try
        {
            // 1. Najważniejszy i szybki fragment ładuje się pierwszy.
            await _appointmentsView.ViewModel.InitializeAsync(ct);

            ct.ThrowIfCancellationRequested();

            // 2. Pozwalamy UI przetworzyć render po pierwszych danych.
            await Task.Yield();

            // 3. Pozostałe niezależne źródła mogą ładować się równolegle.
            await Task.WhenAll(
                _tasksView.ViewModel.InitializeAsync(ct),
                _customersView.ViewModel.InitializeAsync(ct));
        }
        catch (OperationCanceledException)
        {
            // Użytkownik opuścił stronę.
        }
    }

    private void CancelAllLoading()
    {
        var cts = Interlocked.Exchange(ref _pageCts, null);

        if (cts is not null)
        {
            try
            {
                cts.Cancel();
            }
            finally
            {
                cts.Dispose();
            }
        }

        _appointmentsView.ViewModel.CancelLoading();
        _tasksView.ViewModel.CancelLoading();
        _customersView.ViewModel.CancelLoading();
    }

    private void ResetAllChildren()
    {
        CancelAllLoading();

        _appointmentsView.ViewModel.Reset();
        _tasksView.ViewModel.Reset();
        _customersView.ViewModel.Reset();
    }
}
