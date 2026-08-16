namespace MauiAsyncViewsDemo.Views;

/// <summary>
/// Lekki overlay używany tylko podczas przejścia pomiędzy stronami.
/// Pokazuje UI natychmiast i może używać zwykłego spinnera albo własnej animacji.
/// </summary>
public sealed class NavigationTransitionOverlay : Grid
{
    private readonly Border _card;
    private readonly ActivityIndicator _spinner;
    private readonly Label _pulseIcon;
    private readonly Label _message;

    private CancellationTokenSource? _animationCts;

    public NavigationTransitionOverlay()
    {
        IsVisible = false;
        Opacity = 0;
        ZIndex = 10_000;
        BackgroundColor = Color.FromArgb("#66000000");
        InputTransparent = false;

        _pulseIcon = new Label
        {
            Text = "●",
            FontSize = 52,
            TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center,
            Scale = 0.8
        };

        _spinner = new ActivityIndicator
        {
            IsVisible = false,
            IsRunning = false,
            WidthRequest = 44,
            HeightRequest = 44,
            HorizontalOptions = LayoutOptions.Center
        };

        _message = new Label
        {
            TextColor = Colors.White,
            FontSize = 16,
            HorizontalTextAlignment = TextAlignment.Center
        };

        _card = new Border
        {
            BackgroundColor = Color.FromArgb("#DD202020"),
            StrokeThickness = 0,
            Padding = new Thickness(28, 22),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 18
            },
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                MinimumWidthRequest = 220,
                Children =
                {
                    _pulseIcon,
                    _spinner,
                    _message
                }
            }
        };

        Children.Add(_card);
    }

    public async Task ShowAsync(string message, bool useSpinner = false)
    {
        StopAnimation();

        _message.Text = message;
        _spinner.IsVisible = useSpinner;
        _spinner.IsRunning = useSpinner;
        _pulseIcon.IsVisible = !useSpinner;

        IsVisible = true;
        Opacity = 0;
        _card.Scale = 0.94;

        await Task.WhenAll(
            this.FadeToAsync(1, 120, Easing.CubicOut),
            _card.ScaleToAsync(1, 150, Easing.CubicOut));

        if (!useSpinner)
        {
            _animationCts = new CancellationTokenSource();
            _ = RunPulseAnimationAsync(_animationCts.Token);
        }
    }

    public async Task HideAsync()
    {
        StopAnimation();

        if (!IsVisible)
            return;

        await this.FadeToAsync(0, 100, Easing.CubicIn);
        IsVisible = false;
    }

    public void StopAnimation()
    {
        var cts = Interlocked.Exchange(ref _animationCts, null);
        if (cts is not null)
        {
            cts.Cancel();
            cts.Dispose();
        }

        _spinner.IsRunning = false;
    }

    private async Task RunPulseAnimationAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.WhenAll(
                    _pulseIcon.ScaleToAsync(1.15, 330, Easing.CubicInOut),
                    _pulseIcon.FadeToAsync(0.55, 330, Easing.CubicInOut));

                cancellationToken.ThrowIfCancellationRequested();

                await Task.WhenAll(
                    _pulseIcon.ScaleToAsync(0.82, 330, Easing.CubicInOut),
                    _pulseIcon.FadeToAsync(1, 330, Easing.CubicInOut));
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
