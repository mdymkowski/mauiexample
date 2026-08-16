using MauiAsyncViewsDemo.Models;

namespace MauiAsyncViewsDemo.Repositories;

public sealed class FakeTaskRepository : ITaskRepository
{
    public async Task<IReadOnlyList<TaskModel>> GetDashboardTasksAsync(
        CancellationToken cancellationToken)
    {
        await Task.Delay(1300, cancellationToken);

        return Enumerable.Range(1, 30)
            .Select(i => new TaskModel(
                Guid.NewGuid(),
                $"Zadanie {i}",
                DateTime.Today.AddDays(i % 5),
                i % 4 == 0))
            .ToArray();
    }
}
