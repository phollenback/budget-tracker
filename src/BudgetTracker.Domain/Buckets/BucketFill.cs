namespace BudgetTracker.Domain;

public sealed class BucketFill
{
    public AllocationBucket Bucket { get; }
    public Percentage Target { get; }
    public Money TargetAmount { get; }
    public Money Actual { get; }
    public Percentage? ShareOfIncome { get; }
    public int ProgressPercent { get; }
    public bool IsFilled { get; }

    public string Title => Bucket switch
    {
        AllocationBucket.Essential => "Essential",
        AllocationBucket.Discretionary => "Discretionary",
        AllocationBucket.SavingAndInvesting => "Saving and investing",
        _ => throw new ArgumentOutOfRangeException(nameof(Bucket), Bucket, "Unknown allocation bucket.")
    };

    private BucketFill(
        AllocationBucket bucket,
        Percentage target,
        Money targetAmount,
        Money actual,
        Percentage? shareOfIncome,
        int progressPercent,
        bool isFilled)
    {
        Bucket = bucket;
        Target = target;
        TargetAmount = targetAmount;
        Actual = actual;
        ShareOfIncome = shareOfIncome;
        ProgressPercent = progressPercent;
        IsFilled = isFilled;
    }

    public static BucketFill From(BucketAllocation allocation, Money income)
    {
        ArgumentNullException.ThrowIfNull(allocation);
        ArgumentNullException.ThrowIfNull(income);

        var targetAmount = AllocationPolicy.TargetAmount(income, allocation.Target);
        var actual = allocation.Total;
        var filled = targetAmount.Equals(Money.Zero)
            ? !actual.Equals(Money.Zero)
            : actual.Amount >= targetAmount.Amount;

        return new BucketFill(
            allocation.Bucket,
            allocation.Target,
            targetAmount,
            actual,
            allocation.ShareOfIncome,
            ProgressPercentOf(actual, targetAmount),
            filled);
    }

    private static int ProgressPercentOf(Money actual, Money target)
    {
        if (target.Equals(Money.Zero))
        {
            return actual.Equals(Money.Zero) ? 0 : 100;
        }

        var percent = decimal.Round(actual.Amount / target.Amount * 100m, 0, MidpointRounding.AwayFromZero);
        return (int)Math.Clamp(percent, 0m, 100m);
    }
}
