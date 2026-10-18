namespace BudgetTracker.Domain;

public interface IBucketBookRepository
{
    Task<BucketBook> GetAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(BucketBook book, CancellationToken cancellationToken = default);
}
