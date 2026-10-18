namespace BudgetTracker.Domain;

/// <summary>
/// Whether this month funded the 60 / 20 / 20 buckets and each savings line.
/// </summary>
public sealed class BucketFillBoard
{
    public MonthPeriod Period { get; }
    public bool HasWorksheet { get; }
    public bool IsSubmitted { get; }
    public IReadOnlyList<BucketFill> Buckets { get; }
    public IReadOnlyList<SavingsGoalFill> SavingsGoals { get; }

    private BucketFillBoard(
        MonthPeriod period,
        bool hasWorksheet,
        bool isSubmitted,
        IReadOnlyList<BucketFill> buckets,
        IReadOnlyList<SavingsGoalFill> savingsGoals)
    {
        Period = period;
        HasWorksheet = hasWorksheet;
        IsSubmitted = isSubmitted;
        Buckets = buckets;
        SavingsGoals = savingsGoals;
    }

    public static BucketFillBoard Empty(MonthPeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);
        return new BucketFillBoard(period, false, false, [], []);
    }

    public static BucketFillBoard From(MonthlyBudget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);

        var snapshot = budget.CalculateAllocation();
        var buckets = new[]
        {
            BucketFill.From(snapshot.Essential, budget.NetMonthlyIncome),
            BucketFill.From(snapshot.Discretionary, budget.NetMonthlyIncome),
            BucketFill.From(snapshot.SavingAndInvesting, budget.NetMonthlyIncome)
        };

        var saving = budget.CategoryNamed("Saving and Investing")
                     ?? throw new InvalidOperationException("Official chart is missing Saving and Investing.");

        var goals = saving.Lines.Select(SavingsGoalFill.From).ToList();

        return new BucketFillBoard(budget.Period, true, budget.IsSubmitted, buckets, goals);
    }
}
