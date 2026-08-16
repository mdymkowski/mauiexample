using MauiAsyncViewsDemo.Models;

namespace MauiAsyncViewsDemo.Repositories;

public interface ICustomerRepository
{
    Task<IReadOnlyList<CustomerModel>> GetPageAsync(
        int skip,
        int take,
        CancellationToken cancellationToken);
}
