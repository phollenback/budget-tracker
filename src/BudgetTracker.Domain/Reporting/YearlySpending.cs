namespace BudgetTracker.Domain;

/// <summary>
/// Twelve-month read model: income, spend, and submission counts for a year.
/// </summary>
public sealed class YearlySpending
{
    public YearPeriod Year { get; }
    public IReadOnlyList<MonthSpending> Months { get; }
    public Money TotalSpent { get; }
    public Money TotalIncome { get; }
    public int SubmittedCount { get; }

    private YearlySpending(
        YearPeriod year,
        IReadOnlyList<MonthSpending> months,
        Money totalSpent,
        Money totalIncome,
        int submittedCount)
    {
        Year = year;
        Months = months;
        TotalSpent = totalSpent;
        TotalIncome = totalIncome;
        SubmittedCount = submittedCount;
    }

    public static YearlySpending Assemble(YearPeriod year, IEnumerable<MonthlyBudget> budgets)
    {
        ArgumentNullException.ThrowIfNull(year);
        ArgumentNullException.ThrowIfNull(budgets);

        var byMonth = budgets
            .Where(budget => budget.Period.Year == year.Year && budget.HasWorksheetData)
            .ToDictionary(budget => budget.Period.Month);

        var months = new List<MonthSpending>(12);
        foreach (var period in year.Months())
        {
            months.Add(byMonth.TryGetValue(period.Month, out var budget)
                ? MonthSpending.From(budget)
                : MonthSpending.Empty(period));
        }

        var present = months.Where(month => month.HasWorksheet).ToList();
        return new YearlySpending(
            year,
            months,
            Money.Sum(present.Select(month => month.Spent)),
            Money.Sum(present.Select(month => month.Income)),
            months.Count(month => month.IsSubmitted));
    }
}
