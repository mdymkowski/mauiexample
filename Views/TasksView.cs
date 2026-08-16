using CommunityToolkit.Maui.Markup;
using MauiAsyncViewsDemo.Contracts;
using MauiAsyncViewsDemo.Models;
using MauiAsyncViewsDemo.ViewModels;

namespace MauiAsyncViewsDemo.Views;

public sealed class TasksView : ContentView, IAsyncChildView
{
    public TasksViewModel ViewModel { get; }

    IAsyncInitializable IAsyncChildView.ViewModel => ViewModel;

    public TasksView(TasksViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;

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
                    Text = "Zadania",
                    FontSize = 22,
                    FontAttributes = FontAttributes.Bold,
                    Margin = new Thickness(0, 0, 0, 8)
                },

                new CollectionView
                {
                    ItemTemplate = new DataTemplate(() =>
                        new Label
                        {
                            Padding = new Thickness(6)
                        }
                        .Bind(Label.TextProperty, static (TaskModel x) => x.Subject))
                }
                .Row(1)
                .Bind(ItemsView.ItemsSourceProperty, static (TasksViewModel vm) => vm.Tasks),

                new ActivityIndicator
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
                .Row(1)
                .Bind(ActivityIndicator.IsRunningProperty, static (TasksViewModel vm) => vm.IsLoading)
                .Bind(ActivityIndicator.IsVisibleProperty, static (TasksViewModel vm) => vm.IsLoading)
            }
        };
    }
}
