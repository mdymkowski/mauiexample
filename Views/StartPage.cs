namespace MauiAsyncViewsDemo.Views;

public sealed class StartPage : ContentPage
{
    private readonly NavigationTransitionOverlay _transition = new();
    private readonly View _mainContent;
    private int _navigating;

    public StartPage()
    {
        Title = "Start";
        Shell.SetNavBarIsVisible(this, false);

        var openDashboardButton = new Button
        {
            Text = "Otwórz dashboard",
            FontSize = 18,
            Padding = new Thickness(24, 14),
            HorizontalOptions = LayoutOptions.Center
        };

        openDashboardButton.Clicked += OpenDashboardAsync;

        _mainContent = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 18,
            Children =
            {
                new Label
                {
                    Text = ".NET MAUI Async UI Demo",
                    FontSize = 34,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Label
                {
                    Text = "Własna animacja przejścia uruchamia się przed nawigacją. Dashboard pokazuje się zanim zacznie ładować swoje dane.",
                    FontSize = 16,
                    MaximumWidthRequest = 650,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                openDashboardButton
            }
        };

        Content = new Grid
        {
            Padding = 32,
            Children =
            {
                _mainContent,
                _transition
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Reset po powrocie z Dashboardu.
        _transition.StopAnimation();
        _transition.IsVisible = false;
        _transition.Opacity = 0;
        _mainContent.Opacity = 1;
        _mainContent.Scale = 1;
        Volatile.Write(ref _navigating, 0);
    }

    protected override void OnDisappearing()
    {
        _transition.StopAnimation();
        base.OnDisappearing();
    }

    private async void OpenDashboardAsync(object? sender, EventArgs e)
    {
        if (Interlocked.Exchange(ref _navigating, 1) == 1)
            return;

        try
        {
            // 1. Natychmiast reagujemy na kliknięcie własną animacją.
            await _transition.ShowAsync("Otwieranie dashboardu…", useSpinner: false);

            // 2. Delikatnie odsuwamy aktualną zawartość.
            await Task.WhenAll(
                _mainContent.FadeToAsync(0.70, 110, Easing.CubicIn),
                _mainContent.ScaleToAsync(0.985, 110, Easing.CubicIn));

            // 3. Shell NIE wykonuje swojej standardowej animacji,
            //    ponieważ efekt przejścia kontrolujemy sami.
            await Shell.Current.GoToAsync("//dashboard", animate: false);
        }
        catch
        {
            await _transition.HideAsync();
            _mainContent.Opacity = 1;
            _mainContent.Scale = 1;
            Volatile.Write(ref _navigating, 0);
            throw;
        }
    }
}
