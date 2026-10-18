namespace BudgetTracker.Domain;

public sealed class MonthSpending
{
    public MonthPeriod Period { get; }
    public bool HasWorksheet { get; }
    public bool IsSubmitted { get; }
    public Money Spent { get; }
    public Money Income { get; }

    public string MonthName => Period.MonthName;

    private MonthSpending(MonthPeriod period, bool hasWorksheet, bool isSubmitted, Money spent, Money income)
    {
        Period = period;
        HasWorksheet = hasWorksheet;
        IsSubmitted = isSubmitted;
        Spent = spent;
        Income = income;
    }

    public static MonthSpending Empty(MonthPeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);
        return new MonthSpending(period, false, false, Money.Zero, Money.Zero);
    }

    public static MonthSpending From(MonthlyBudget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);
        var allocation = budget.CalculateAllocation();
        return new MonthSpending(
            budget.Period,
            hasWorksheet: true,
            budget.IsSubmitted,
            allocation.TotalMonthlyExpenses,
            budget.NetMonthlyIncome);
    }
}
