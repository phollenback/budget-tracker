using BudgetTracker.Domain;

namespace BudgetTracker.Application.Tests;

internal sealed class InMemoryBucketBookRepository : IBucketBookRepository
{
    private BucketBook _book = BucketBook.Empty();

    public Task<BucketBook> GetAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_book);

    public Task SaveAsync(BucketBook book, CancellationToken cancellationToken = default)
    {
        _book = book;
        return Task.CompletedTask;
    }
}
