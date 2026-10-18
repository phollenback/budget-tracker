using BudgetTracker.Domain;

namespace BudgetTracker.Application;

public sealed class BudgetWorkspace
{
    private readonly IMonthlyBudgetRepository _repository;
    private readonly IBucketBookRepository _buckets;

    public BudgetWorkspace(IMonthlyBudgetRepository repository, IBucketBookRepository buckets)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _buckets = buckets ?? throw new ArgumentNullException(nameof(buckets));
    }

    public async Task<MonthlyBudget> GetOrOpenAsync(
        MonthPeriod period,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(period);

        var existing = await _repository.GetAsync(period, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var opened = MonthlyBudget.Open(period);
        var ownerName = await _repository.LatestOwnerNameAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(ownerName))
        {
            opened.SetOwnerName(ownerName);
        }

        return opened;
    }

    public Task SaveDraftAsync(MonthlyBudget budget, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(budget);
        return _repository.SaveAsync(budget, cancellationToken);
    }

    public Task SubmitAsync(MonthlyBudget budget, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(budget);
        budget.Submit();
        return _repository.SaveAsync(budget, cancellationToken);
    }

    public async Task<YearlySpending> GetYearAsync(
        YearPeriod year,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(year);
        var months = await _repository.ListForYearAsync(year, cancellationToken);
        return YearlySpending.Assemble(year, months);
    }

    public async Task<BucketFillBoard> GetBucketFillAsync(
        MonthPeriod period,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(period);

        var budget = await _repository.GetAsync(period, cancellationToken);
        if (budget is null || !budget.HasWorksheetData)
        {
            return BucketFillBoard.Empty(period);
        }

        return BucketFillBoard.From(budget);
    }

    public Task<IReadOnlyList<SavedMonth>> ListSavedMonthsAsync(
        CancellationToken cancellationToken = default) =>
        _repository.ListSavedAsync(cancellationToken);

    public async Task<IReadOnlyList<MonthBucketFill>> GetManualBucketsAsync(
        MonthPeriod period,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(period);
        var book = await _buckets.GetAsync(cancellationToken);
        return book.BoardFor(period);
    }

    public async Task AddRecurringBucketAsync(
        string name,
        Money monthlyTarget,
        CancellationToken cancellationToken = default)
    {
        var book = await _buckets.GetAsync(cancellationToken);
        book.AddRecurring(name, monthlyTarget);
        await _buckets.SaveAsync(book, cancellationToken);
    }

    public async Task AddTemporaryBucketAsync(
        MonthPeriod period,
        string name,
        Money monthlyTarget,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(period);
        var book = await _buckets.GetAsync(cancellationToken);
        book.AddTemporary(period, name, monthlyTarget);
        await _buckets.SaveAsync(book, cancellationToken);
    }

    public async Task ContributeToBucketAsync(
        Guid bucketId,
        MonthPeriod period,
        Money amount,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(period);
        var book = await _buckets.GetAsync(cancellationToken);
        book.Contribute(bucketId, period, amount);
        await _buckets.SaveAsync(book, cancellationToken);
    }
}
