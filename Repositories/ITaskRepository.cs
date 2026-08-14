using MauiAsyncViewsDemo.Models;

namespace MauiAsyncViewsDemo.Repositories;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskModel>> GetDashboardTasksAsync(
        CancellationToken cancellationToken);
}
