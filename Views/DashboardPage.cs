using CommunityToolkit.Maui.Markup;

namespace MauiAsyncViewsDemo.Views;

public sealed class DashboardPage : ContentPage
{
    private readonly AppointmentsView _appointmentsView;
    private readonly TasksView _tasksView;
    private readonly CustomersView _customersView;
    private readonly HeavyDataPopupView _heavyDataPopup;
    private readonly NavigationTransitionOverlay _transition = new();

    private readonly Grid _dashboard;
    private CancellationTokenSource? _pageCts;
    private bool _initializationStarted;
    private int _navigatingBack;

    public DashboardPage(IServiceProvider services)
    {
        Title = "Dashboard";

        _appointmentsView = services.GetRequiredService<AppointmentsView>();
        _tasksView = services.GetRequiredService<TasksView>();
        _customersView = services.GetRequiredService<CustomersView>();
        _heavyDataPopup = services.GetRequiredService<HeavyDataPopupView>();

        _dashboard = BuildDashboard();

        // Strona docelowa zaczyna lekko przezroczysta i powiększa się do 1.0.
        _dashboard.Opacity = 0;
        _dashboard.Scale = 0.985;

        Content = new Grid
        {
            Children =
            {
                _dashboard,
                _heavyDataPopup,
                _transition
            }
        };
    }

    private Grid BuildDashboard()
    {
        var backButton = new Button { Text = "← Start" };
        backButton.Clicked += BackToStartAsync;

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
                                    Text = "Dashboard najpierw wykonuje fade-in, potem dopiero uruchamia LoadData child ViewModeli."
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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _transition.StopAnimation();
        _transition.IsVisible = false;

        // Najpierw animujemy wejście strony. To nie zależy od danych.
        _dashboard.Opacity = 0;
        _dashboard.Scale = 0.985;

        await Task.WhenAll(
            _dashboard.FadeToAsync(1, 180, Easing.CubicOut),
            _dashboard.ScaleToAsync(1, 180, Easing.CubicOut));

        if (_initializationStarted)
            return;

        _initializationStarted = true;

        // Dopiero po pokazaniu strony rozpoczynamy pobieranie danych.
        await Task.Yield();
        await StartLoadingAsync();
    }

    protected override void OnDisappearing()
    {
        _transition.StopAnimation();
        CancelAllLoading();
        _heavyDataPopup.ViewModel.Cancel();
        base.OnDisappearing();
    }

    private async void BackToStartAsync(object? sender, EventArgs e)
    {
        if (Interlocked.Exchange(ref _navigatingBack, 1) == 1)
            return;

        try
        {
            CancelAllLoading();

            // Tu dla porównania używamy klasycznego ActivityIndicator.
            await _transition.ShowAsync("Powrót do strony startowej…", useSpinner: true);

            await Task.WhenAll(
                _dashboard.FadeToAsync(0.65, 100, Easing.CubicIn),
                _dashboard.ScaleToAsync(0.985, 100, Easing.CubicIn));

            await Shell.Current.GoToAsync("//start", animate: false);
        }
        catch
        {
            await _transition.HideAsync();
            _dashboard.Opacity = 1;
            _dashboard.Scale = 1;
            Volatile.Write(ref _navigatingBack, 0);
            throw;
        }
    }

    private async Task StartLoadingAsync()
    {
        CancelAllLoading();

        _pageCts = new CancellationTokenSource();
        var ct = _pageCts.Token;

        try
        {
            await _appointmentsView.ViewModel.InitializeAsync(ct);
            ct.ThrowIfCancellationRequested();

            await Task.Yield();

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
