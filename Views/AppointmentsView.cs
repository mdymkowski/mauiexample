using CommunityToolkit.Maui.Markup;
using MauiAsyncViewsDemo.Contracts;
using MauiAsyncViewsDemo.Models;
using MauiAsyncViewsDemo.ViewModels;

namespace MauiAsyncViewsDemo.Views;

public sealed class AppointmentsView : ContentView, IAsyncChildView
{
    public AppointmentsViewModel ViewModel { get; }

    IAsyncInitializable IAsyncChildView.ViewModel => ViewModel;

    public AppointmentsView(AppointmentsViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;

        // WAŻNE: konstruktor tylko buduje UI. Zero LoadData().
        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            },

            Children =
            {
                new Label
                {
                    Text = "Dzisiejsze spotkania",
                    FontSize = 22,
                    FontAttributes = FontAttributes.Bold,
                    Margin = new Thickness(0, 0, 0, 8)
                },

                new CollectionView
                {
                    ItemTemplate = new DataTemplate(() =>
                    {
                        var subject = new Label { FontAttributes = FontAttributes.Bold }
                            .Bind(Label.TextProperty, static (AppointmentModel x) => x.Subject);

                        var customer = new Label { FontSize = 12 }
                            .Bind(Label.TextProperty, static (AppointmentModel x) => x.CustomerName);

                        var start = new Label { FontSize = 12 }
                            .Bind(Label.TextProperty, static (AppointmentModel x) => x.Start, stringFormat: "{0:HH:mm}");

                        return new Grid
                        {
                            ColumnDefinitions =
                            {
                                new ColumnDefinition(GridLength.Star),
                                new ColumnDefinition(GridLength.Auto)
                            },
                            Padding = new Thickness(6),
                            Children =
                            {
                                new VerticalStackLayout
                                {
                                    Children = { subject, customer }
                                },
                                start.Column(1)
                            }
                        };
                    })
                }
                .Row(1)
                .Bind(ItemsView.ItemsSourceProperty, static (AppointmentsViewModel vm) => vm.Appointments),

                new ActivityIndicator
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
                .Row(1)
                .Bind(ActivityIndicator.IsRunningProperty, static (AppointmentsViewModel vm) => vm.IsLoading)
                .Bind(ActivityIndicator.IsVisibleProperty, static (AppointmentsViewModel vm) => vm.IsLoading)
            }
        };
    }
}
