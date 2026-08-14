using MauiAsyncViewsDemo.Repositories;
using MauiAsyncViewsDemo.ViewModels;
using MauiAsyncViewsDemo.Views;
using Microsoft.Extensions.Logging;

namespace MauiAsyncViewsDemo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseMauiApp<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<ICustomerRepository, FakeCustomerRepository>();
        builder.Services.AddSingleton<ITaskRepository, FakeTaskRepository>();
        builder.Services.AddSingleton<IAppointmentRepository, FakeAppointmentRepository>();
        builder.Services.AddSingleton<IHeavyDataRepository, FakeHeavyDataRepository>();

        builder.Services.AddTransient<CustomersViewModel>();
        builder.Services.AddTransient<TasksViewModel>();
        builder.Services.AddTransient<AppointmentsViewModel>();
        builder.Services.AddTransient<HeavyDataPopupViewModel>();

        builder.Services.AddTransient<CustomersView>();
        builder.Services.AddTransient<TasksView>();
        builder.Services.AddTransient<AppointmentsView>();
        builder.Services.AddTransient<HeavyDataPopupView>();

        builder.Services.AddTransient<StartPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
