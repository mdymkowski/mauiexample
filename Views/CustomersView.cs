using CommunityToolkit.Maui.Markup;
using MauiAsyncViewsDemo.Contracts;
using MauiAsyncViewsDemo.Models;
using MauiAsyncViewsDemo.ViewModels;

namespace MauiAsyncViewsDemo.Views;

public sealed class CustomersView : ContentView, IAsyncChildView
{
    public CustomersViewModel ViewModel { get; }

    IAsyncInitializable IAsyncChildView.ViewModel => ViewModel;

    public CustomersView(CustomersViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;

        var collection = new CollectionView
        {
            RemainingItemsThreshold = 10,

            ItemTemplate = new DataTemplate(() =>
            {
                var name = new Label { FontAttributes = FontAttributes.Bold }
                    .Bind(Label.TextProperty, static (CustomerModel x) => x.Name);

                var city = new Label { FontSize = 12 }
                    .Bind(Label.TextProperty, static (CustomerModel x) => x.City);

                return new VerticalStackLayout
                {
                    Padding = new Thickness(6),
                    Children = { name, city }
                };
            })
        }
        .Bind(ItemsView.ItemsSourceProperty, static (CustomersViewModel vm) => vm.Customers)
        .Bind(
            CollectionView.RemainingItemsThresholdReachedCommandProperty,
            static (CustomersViewModel vm) => vm.LoadMoreCommand);

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },

            Children =
            {
                new Label
                {
                    Text = "Klienci - paginacja 50 rekordów",
                    FontSize = 22,
                    FontAttributes = FontAttributes.Bold,
                    Margin = new Thickness(0, 0, 0, 8)
                },

                collection.Row(1),

                new ActivityIndicator
                {
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 6)
                }
                .Row(2)
                .Bind(ActivityIndicator.IsRunningProperty, static (CustomersViewModel vm) => vm.IsLoadingMore)
                .Bind(ActivityIndicator.IsVisibleProperty, static (CustomersViewModel vm) => vm.IsLoadingMore),

                new ActivityIndicator
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
                .Row(1)
                .Bind(ActivityIndicator.IsRunningProperty, static (CustomersViewModel vm) => vm.IsLoading)
                .Bind(ActivityIndicator.IsVisibleProperty, static (CustomersViewModel vm) => vm.IsLoading)
            }
        };
    }
}
