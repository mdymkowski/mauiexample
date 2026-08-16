using MauiAsyncViewsDemo.Models;

namespace MauiAsyncViewsDemo.Repositories;

public sealed class FakeCustomerRepository : ICustomerRepository
{
    public async Task<IReadOnlyList<CustomerModel>> GetPageAsync(
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // Symulacja SQLite/API.
        await Task.Delay(700, cancellationToken);

        const int total = 1000;

        if (skip >= total)
            return [];

        var count = Math.Min(take, total - skip);

        return Enumerable.Range(skip + 1, count)
            .Select(i => new CustomerModel(
                Guid.NewGuid(),
                $"Klient {i:0000}",
                (i % 3) switch
                {
                    0 => "Poznań",
                    1 => "Warszawa",
                    _ => "Wrocław"
                }))
            .ToArray();
    }
}
