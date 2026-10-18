namespace BudgetTracker.Domain;

public sealed class FundingBucket
{
    public const int PaletteSize = 8;

    public Guid Id { get; }
    public string Name { get; }
    public Money MonthlyTarget { get; }
    public int ColorIndex { get; }
    public BucketLifespan Lifespan { get; }
    public MonthPeriod? OnlyIn { get; }

    public bool IsTemporary => Lifespan == BucketLifespan.ThisMonthOnly;

    public static FundingBucket Restore(
        Guid id,
        string name,
        Money monthlyTarget,
        int colorIndex,
        BucketLifespan lifespan,
        MonthPeriod? onlyIn)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Bucket name is required.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(monthlyTarget);

        if (lifespan == BucketLifespan.ThisMonthOnly)
        {
            ArgumentNullException.ThrowIfNull(onlyIn);
        }

        return new FundingBucket(
            id,
            name.Trim(),
            monthlyTarget,
            Math.Clamp(colorIndex, 0, PaletteSize - 1),
            lifespan,
            lifespan == BucketLifespan.ThisMonthOnly ? onlyIn : null);
    }

    internal FundingBucket(
        Guid id,
        string name,
        Money monthlyTarget,
        int colorIndex,
        BucketLifespan lifespan,
        MonthPeriod? onlyIn)
    {
        Id = id;
        Name = name;
        MonthlyTarget = monthlyTarget;
        ColorIndex = colorIndex;
        Lifespan = lifespan;
        OnlyIn = onlyIn;
    }

    public bool AppearsIn(MonthPeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);
        return Lifespan == BucketLifespan.Recurring || OnlyIn == period;
    }
}
