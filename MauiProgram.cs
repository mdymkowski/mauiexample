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

        // Repository - w realnej aplikacji może to być SQLite/API/EF Core.
        builder.Services.AddSingleton<ICustomerRepository, FakeCustomerRepository>();
        builder.Services.AddSingleton<ITaskRepository, FakeTaskRepository>();
        builder.Services.AddSingleton<IAppointmentRepository, FakeAppointmentRepository>();

        // Child ViewModels. Transient jest OK, ponieważ child View jest tworzony raz
        // i przechowuje swoją instancję ViewModelu.
        builder.Services.AddTransient<CustomersViewModel>();
        builder.Services.AddTransient<TasksViewModel>();
        builder.Services.AddTransient<AppointmentsViewModel>();

        // Child Views.
        builder.Services.AddTransient<CustomersView>();
        builder.Services.AddTransient<TasksView>();
        builder.Services.AddTransient<AppointmentsView>();

        // Pages.
        builder.Services.AddTransient<DashboardPage>();

        return builder.Build();
    }
}
