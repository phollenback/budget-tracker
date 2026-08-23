namespace BudgetTracker.Domain;

public sealed class BucketAllocation
{
    public AllocationBucket Bucket { get; }
    public Money Total { get; }
    public Percentage Target { get; }
    public Percentage? ShareOfIncome { get; }

    /// <summary>
    /// Actual share minus target, in points. Null when income is zero.
    /// </summary>
    public decimal? VariancePoints { get; }

    internal BucketAllocation(AllocationBucket bucket, Money total, Money income)
    {
        Bucket = bucket;
        Total = total;
        Target = AllocationPolicy.TargetFor(bucket);
        ShareOfIncome = Percentage.ShareOf(total, income);
        VariancePoints = ShareOfIncome is null
            ? null
            : ShareOfIncome.Points - Target.Points;
    }
}
