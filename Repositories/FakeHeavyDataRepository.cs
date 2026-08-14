using MauiAsyncViewsDemo.Models;

namespace MauiAsyncViewsDemo.Repositories;

public sealed class FakeHeavyDataRepository : IHeavyDataRepository
{
    public async Task<IReadOnlyList<HeavyDataItem>> GetLargeDataSetAsync(
        CancellationToken cancellationToken)
    {
        // Symulujemy wolne API / SQLite. Popup powinien być już wtedy widoczny.
        await Task.Delay(1800, cancellationToken);

        return Enumerable.Range(1, 500)
            .Select(i => new HeavyDataItem(
                i,
                $"Rekord {i:0000}",
                $"Przykładowe dane elementu {i}"))
            .ToArray();
    }
}
