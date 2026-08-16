using CommunityToolkit.Maui.Markup;
using MauiAsyncViewsDemo.Models;
using MauiAsyncViewsDemo.ViewModels;

namespace MauiAsyncViewsDemo.Views;

public sealed class HeavyDataPopupView : Grid
{
    private readonly Border _card;
    private CancellationTokenSource? _openCts;

    public HeavyDataPopupViewModel ViewModel { get; }

    public HeavyDataPopupView(HeavyDataPopupViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;

        IsVisible = false;
        Opacity = 0;
        ZIndex = 1000;
        BackgroundColor = Color.FromArgb("#99000000");

        var closeButton = new Button
        {
            Text = "Zamknij",
            HorizontalOptions = LayoutOptions.End
        };
        closeButton.Clicked += async (_, _) => await CloseAsync();

        var list = new CollectionView
        {
            ItemTemplate = new DataTemplate(() =>
            {
                var name = new Label { FontAttributes = FontAttributes.Bold }
                    .Bind(Label.TextProperty, static (HeavyDataItem x) => x.Name);
                var description = new Label { FontSize = 12 }
                    .Bind(Label.TextProperty, static (HeavyDataItem x) => x.Description);

                return new VerticalStackLayout
                {
                    Padding = new Thickness(6),
                    Children = { name, description }
                };
            })
        }
        .Bind(ItemsView.ItemsSourceProperty, static (HeavyDataPopupViewModel vm) => vm.Items);

        _card = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Padding = 20,
            Margin = 24,
            MaximumWidthRequest = 760,
            MaximumHeightRequest = 720,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 18
            },
            Content = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto)
                },
                RowSpacing = 12,
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
                            new Label
                            {
                                Text = "Duży zestaw danych",
                                FontSize = 24,
                                FontAttributes = FontAttributes.Bold
                            },
                            closeButton.Column(1)
                        }
                    },
                    new Label
                    {
                        Text = "Popup pojawia się natychmiast. Dane są ładowane dopiero po jego wyrenderowaniu."
                    }.Row(1),
                    list.Row(2),
                    new VerticalStackLayout
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new ActivityIndicator()
                                .Bind(ActivityIndicator.IsRunningProperty, static (HeavyDataPopupViewModel vm) => vm.IsLoading)
                                .Bind(ActivityIndicator.IsVisibleProperty, static (HeavyDataPopupViewModel vm) => vm.IsLoading),
                            new Label { Text = "Ładowanie 500 rekordów..." }
                                .Bind(Label.IsVisibleProperty, static (HeavyDataPopupViewModel vm) => vm.IsLoading)
                        }
                    }.Row(3)
                }
            }
        };

        Children.Add(_card);
    }

    public async Task OpenAsync()
    {
        if (IsVisible)
            return;

        _openCts?.Cancel();
        _openCts?.Dispose();
        _openCts = new CancellationTokenSource();

        // Najpierw pokazujemy gotowy, lekki UI popupu.
        IsVisible = true;
        Opacity = 0;
        _card.Scale = 0.96;

        await Task.WhenAll(
            this.FadeTo(1, 120, Easing.CubicOut),
            _card.ScaleTo(1, 120, Easing.CubicOut));

        // Dajemy rendererowi zakończyć klatkę przed cięższym I/O.
        await Task.Yield();

        // Dopiero teraz rozpoczyna się pobieranie danych.
        await ViewModel.LoadAsync(_openCts.Token);
    }

    public async Task CloseAsync()
    {
        if (!IsVisible)
            return;

        _openCts?.Cancel();
        ViewModel.Cancel();

        await this.FadeTo(0, 90, Easing.CubicIn);
        IsVisible = false;

        _openCts?.Dispose();
        _openCts = null;
    }
}
