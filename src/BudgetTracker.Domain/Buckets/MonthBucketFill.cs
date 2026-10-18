namespace BudgetTracker.Domain;

public sealed class MonthBucketFill
{
    public Guid Id { get; }
    public string Name { get; }
    public Money Target { get; }
    public Money Funded { get; }
    public int ColorIndex { get; }
    public bool IsTemporary { get; }
    public int ProgressPercent { get; }
    public bool IsFilled { get; }

    internal MonthBucketFill(
        Guid id,
        string name,
        Money target,
        Money funded,
        int colorIndex,
        bool isTemporary)
    {
        Id = id;
        Name = name;
        Target = target;
        Funded = funded;
        ColorIndex = colorIndex;
        IsTemporary = isTemporary;
        ProgressPercent = ProgressOf(funded, target);
        IsFilled = target.Equals(Money.Zero)
            ? !funded.Equals(Money.Zero)
            : funded.Amount >= target.Amount;
    }

    private static int ProgressOf(Money funded, Money target)
    {
        if (target.Equals(Money.Zero))
        {
            return funded.Equals(Money.Zero) ? 0 : 100;
        }

        var percent = decimal.Round(funded.Amount / target.Amount * 100m, 0, MidpointRounding.AwayFromZero);
        return (int)Math.Clamp(percent, 0m, 100m);
    }
}
