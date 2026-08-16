using MauiAsyncViewsDemo.Models;

namespace MauiAsyncViewsDemo.Repositories;

public interface IHeavyDataRepository
{
    Task<IReadOnlyList<HeavyDataItem>> GetLargeDataSetAsync(
        CancellationToken cancellationToken);
}
