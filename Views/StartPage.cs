namespace MauiAsyncViewsDemo.Views;

public sealed class StartPage : ContentPage
{
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

        Content = new Grid
        {
            Padding = 32,
            Children =
            {
                new VerticalStackLayout
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
                            Text = "Strona startowa niczego ciężkiego nie ładuje. Kliknięcie najpierw nawiguje do Dashboardu, a dane są pobierane dopiero po pojawieniu się widoku.",
                            FontSize = 16,
                            MaximumWidthRequest = 650,
                            HorizontalTextAlignment = TextAlignment.Center
                        },
                        openDashboardButton
                    }
                }
            }
        };
    }

    private async void OpenDashboardAsync(object? sender, EventArgs e)
    {
        if (Interlocked.Exchange(ref _navigating, 1) == 1)
            return;

        try
        {
            // Nie pobieramy tutaj danych Dashboardu. Shell może od razu wykonać animację.
            await Shell.Current.GoToAsync("//dashboard", animate: true);
        }
        finally
        {
            Volatile.Write(ref _navigating, 0);
        }
    }
}
