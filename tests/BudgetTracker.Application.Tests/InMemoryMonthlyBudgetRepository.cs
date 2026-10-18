using BudgetTracker.Application;
using BudgetTracker.Domain;

namespace BudgetTracker.Application.Tests;

internal sealed class InMemoryMonthlyBudgetRepository : IMonthlyBudgetRepository
{
    private readonly Dictionary<(int Year, int Month), MonthlyBudget> _store = new();

    public Task<MonthlyBudget?> GetAsync(MonthPeriod period, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue((period.Year, period.Month), out var budget);
        return Task.FromResult(budget);
    }

    public Task SaveAsync(MonthlyBudget budget, CancellationToken cancellationToken = default)
    {
        _store[(budget.Period.Year, budget.Period.Month)] = budget;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<MonthlyBudget>> ListForYearAsync(
        YearPeriod year,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MonthlyBudget> budgets = _store.Values
            .Where(budget => budget.Period.Year == year.Year)
            .OrderBy(budget => budget.Period.Month)
            .ToList();
        return Task.FromResult(budgets);
    }

    public Task<IReadOnlyList<SavedMonth>> ListSavedAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SavedMonth> saved = _store.Values
            .Where(budget => budget.HasWorksheetData)
            .Select(budget => new SavedMonth(budget.Period, budget.IsSubmitted, budget.SubmittedAt))
            .OrderByDescending(month => month.Period.Start)
            .ToList();
        return Task.FromResult(saved);
    }

    public Task<string?> LatestOwnerNameAsync(CancellationToken cancellationToken = default)
    {
        var name = _store.Values
            .OrderByDescending(budget => budget.Period.Start)
            .Select(budget => budget.OwnerName)
            .FirstOrDefault(owner => !string.IsNullOrWhiteSpace(owner));
        return Task.FromResult(name);
    }
}
