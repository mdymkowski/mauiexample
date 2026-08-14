namespace MauiAsyncViewsDemo;

public sealed class App : Application
{
    private readonly AppShell _shell;

    public App(AppShell shell)
    {
        _shell = shell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(_shell);
}
