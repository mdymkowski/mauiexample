using MauiAsyncViewsDemo.Views;

namespace MauiAsyncViewsDemo;

public sealed class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var dashboard = _services.GetRequiredService<DashboardPage>();
        return new Window(new NavigationPage(dashboard));
    }
}
