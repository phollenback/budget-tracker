namespace BudgetTracker.Domain;

/// <summary>
/// The worksheet's 60 / 20 / 20 targets.
/// </summary>
public static class AllocationPolicy
{
    public static Percentage EssentialTarget { get; } = Percentage.FromPoints(60);
    public static Percentage DiscretionaryTarget { get; } = Percentage.FromPoints(20);
    public static Percentage SavingTarget { get; } = Percentage.FromPoints(20);

    public static Percentage TargetFor(AllocationBucket bucket) =>
        bucket switch
        {
            AllocationBucket.Essential => EssentialTarget,
            AllocationBucket.Discretionary => DiscretionaryTarget,
            AllocationBucket.SavingAndInvesting => SavingTarget,
            _ => throw new ArgumentOutOfRangeException(nameof(bucket), bucket, "Unknown allocation bucket.")
        };

    public static Money TargetAmount(Money income, Percentage target)
    {
        ArgumentNullException.ThrowIfNull(income);
        ArgumentNullException.ThrowIfNull(target);

        var amount = decimal.Round(income.Amount * target.Points / 100m, 2, MidpointRounding.AwayFromZero);
        return Money.From(amount);
    }
}
