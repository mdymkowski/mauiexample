using CommunityToolkit.Maui.Markup;

namespace MauiAsyncViewsDemo.Views;

public sealed class DashboardPage : ContentPage
{
    private readonly AppointmentsView _appointmentsView;
    private readonly TasksView _tasksView;
    private readonly CustomersView _customersView;
    private readonly HeavyDataPopupView _heavyDataPopup;

    private CancellationTokenSource? _pageCts;
    private bool _initializationStarted;

    public DashboardPage(IServiceProvider services)
    {
        Title = "Dashboard";

        // DI tworzy drzewo widoków, ale żaden konstruktor nie pobiera danych.
        _appointmentsView = services.GetRequiredService<AppointmentsView>();
        _tasksView = services.GetRequiredService<TasksView>();
        _customersView = services.GetRequiredService<CustomersView>();
        _heavyDataPopup = services.GetRequiredService<HeavyDataPopupView>();

        Content = BuildLayout();
    }

    private View BuildLayout()
    {
        var backButton = new Button { Text = "← Start" };
        backButton.Clicked += async (_, _) =>
        {
            CancelAllLoading();
            await Shell.Current.GoToAsync("//start", animate: true);
        };

        var popupButton = new Button
        {
            Text = "Otwórz popup z dużą ilością danych"
        };
        popupButton.Clicked += async (_, _) => await _heavyDataPopup.OpenAsync();

        var reloadButton = new Button
        {
            Text = "Reset + załaduj ponownie"
        };
        reloadButton.Clicked += async (_, _) =>
        {
            ResetAllChildren();
            await StartLoadingAsync();
        };

        var dashboard = new Grid
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
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Auto)
                    },
                    ColumnSpacing = 8,
                    Children =
                    {
                        backButton,
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
                                    Text = "Shell pokazuje stronę od razu, a dane child viewów startują dopiero po OnAppearing."
                                }
                            }
                        }.Column(1),
                        popupButton.Column(2),
                        reloadButton.Column(3)
                    }
                }.ColumnSpan(2),

                _appointmentsView.Row(1).Column(0),
                _tasksView.Row(1).Column(1),
                new Border
                {
                    StrokeThickness = 1,
                    Padding = 10,
                    Content = _customersView
                }.Row(2).ColumnSpan(2)
            }
        };

        // Overlay jest już częścią drzewa UI. Jego pokazanie nie wymaga tworzenia
        // całego widoku ani pobrania danych.
        return new Grid
        {
            Children =
            {
                dashboard,
                _heavyDataPopup
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_initializationStarted)
            return;

        _initializationStarted = true;

        // Shell kończy przejście i ma szansę narysować Dashboard.
        await Task.Yield();
        await StartLoadingAsync();
    }

    protected override void OnDisappearing()
    {
        CancelAllLoading();
        _heavyDataPopup.ViewModel.Cancel();
        base.OnDisappearing();
    }

    private async Task StartLoadingAsync()
    {
        CancelAllLoading();

        _pageCts = new CancellationTokenSource();
        var ct = _pageCts.Token;

        try
        {
            // Widoczny / najważniejszy fragment jako pierwszy.
            await _appointmentsView.ViewModel.InitializeAsync(ct);
            ct.ThrowIfCancellationRequested();

            await Task.Yield();

            // Niezależne źródła danych dopiero później.
            await Task.WhenAll(
                _tasksView.ViewModel.InitializeAsync(ct),
                _customersView.ViewModel.InitializeAsync(ct));
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void CancelAllLoading()
    {
        var cts = Interlocked.Exchange(ref _pageCts, null);

        if (cts is not null)
        {
            try { cts.Cancel(); }
            finally { cts.Dispose(); }
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
