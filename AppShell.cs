using MauiAsyncViewsDemo.Views;

namespace MauiAsyncViewsDemo;

public sealed class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        FlyoutBehavior = FlyoutBehavior.Disabled;

        Items.Add(new ShellContent
        {
            Route = "start",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<StartPage>())
        });

        Items.Add(new ShellContent
        {
            Route = "dashboard",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<DashboardPage>())
        });
    }
}
