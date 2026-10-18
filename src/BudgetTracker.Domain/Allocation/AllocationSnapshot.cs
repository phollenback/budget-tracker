namespace BudgetTracker.Domain;

public sealed class AllocationSnapshot
{
    public BucketAllocation Essential { get; }
    public BucketAllocation Discretionary { get; }
    public BucketAllocation SavingAndInvesting { get; }
    public Money TotalMonthlyExpenses { get; }
    public Money LessExpenses { get; }
    public Balance SurplusOrDeficit { get; }

    internal AllocationSnapshot(
        BucketAllocation essential,
        BucketAllocation discretionary,
        BucketAllocation savingAndInvesting,
        Money totalMonthlyExpenses,
        Balance surplusOrDeficit)
    {
        Essential = essential;
        Discretionary = discretionary;
        SavingAndInvesting = savingAndInvesting;
        TotalMonthlyExpenses = totalMonthlyExpenses;
        LessExpenses = totalMonthlyExpenses;
        SurplusOrDeficit = surplusOrDeficit;
    }
}
