using MauiAsyncViewsDemo.Models;

namespace MauiAsyncViewsDemo.Repositories;

public sealed class FakeAppointmentRepository : IAppointmentRepository
{
    public async Task<IReadOnlyList<AppointmentModel>> GetTodayAsync(
        CancellationToken cancellationToken)
    {
        await Task.Delay(400, cancellationToken);

        return Enumerable.Range(1, 8)
            .Select(i => new AppointmentModel(
                Guid.NewGuid(),
                $"Spotkanie {i}",
                DateTime.Today.AddHours(8 + i),
                $"Klient {i}"))
            .ToArray();
    }
}
