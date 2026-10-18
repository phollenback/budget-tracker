namespace BudgetTracker.Domain;

public interface IMonthlyBudgetRepository
{
    Task<MonthlyBudget?> GetAsync(MonthPeriod period, CancellationToken cancellationToken = default);

    Task SaveAsync(MonthlyBudget budget, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlyBudget>> ListForYearAsync(YearPeriod year, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SavedMonth>> ListSavedAsync(CancellationToken cancellationToken = default);

    Task<string?> LatestOwnerNameAsync(CancellationToken cancellationToken = default);
}
