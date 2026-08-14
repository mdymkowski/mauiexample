using MauiAsyncViewsDemo.Models;

namespace MauiAsyncViewsDemo.Repositories;

public interface IAppointmentRepository
{
    Task<IReadOnlyList<AppointmentModel>> GetTodayAsync(
        CancellationToken cancellationToken);
}
