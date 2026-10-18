namespace BudgetTracker.Domain;

/// <summary>
/// Recurring envelopes plus one-month buckets, with a separate fill total per month.
/// </summary>
public sealed class BucketBook
{
    private readonly List<FundingBucket> _buckets;
    private readonly Dictionary<(Guid BucketId, int Year, int Month), Money> _funded;

    public IReadOnlyList<FundingBucket> Buckets => _buckets;

    public IReadOnlyList<BucketMonthFund> Funds =>
        _funded
            .Select(entry => new BucketMonthFund(
                entry.Key.BucketId,
                MonthPeriod.From(entry.Key.Year, entry.Key.Month),
                entry.Value))
            .ToList();

    private BucketBook()
    {
        _buckets = [];
        _funded = [];
    }

    public static BucketBook Empty() => new();

    public static BucketBook Restore(
        IEnumerable<FundingBucket> buckets,
        IEnumerable<BucketMonthFund> funds)
    {
        ArgumentNullException.ThrowIfNull(buckets);
        ArgumentNullException.ThrowIfNull(funds);

        var book = new BucketBook();
        book._buckets.AddRange(buckets);
        foreach (var fund in funds)
        {
            book._funded[(fund.BucketId, fund.Period.Year, fund.Period.Month)] = fund.Amount;
        }

        return book;
    }

    public FundingBucket AddRecurring(string name, Money monthlyTarget)
    {
        var bucket = CreateBucket(name, monthlyTarget, BucketLifespan.Recurring, onlyIn: null);
        _buckets.Add(bucket);
        return bucket;
    }

    public FundingBucket AddTemporary(MonthPeriod period, string name, Money monthlyTarget)
    {
        ArgumentNullException.ThrowIfNull(period);
        var bucket = CreateBucket(name, monthlyTarget, BucketLifespan.ThisMonthOnly, period);
        _buckets.Add(bucket);
        return bucket;
    }

    public void Contribute(Guid bucketId, MonthPeriod period, Money amount)
    {
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(amount);

        var bucket = Find(bucketId)
                     ?? throw new ArgumentException($"Unknown bucket '{bucketId}'.", nameof(bucketId));

        if (!bucket.AppearsIn(period))
        {
            throw new InvalidOperationException($"Bucket '{bucket.Name}' is not available in {period}.");
        }

        if (amount.Equals(Money.Zero))
        {
            return;
        }

        var key = (bucketId, period.Year, period.Month);
        _funded[key] = _funded.TryGetValue(key, out var current)
            ? current.Add(amount)
            : amount;
    }

    public IReadOnlyList<MonthBucketFill> BoardFor(MonthPeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);

        return _buckets
            .Where(bucket => bucket.AppearsIn(period))
            .Select(bucket => new MonthBucketFill(
                bucket.Id,
                bucket.Name,
                bucket.MonthlyTarget,
                FundedIn(bucket.Id, period),
                bucket.ColorIndex,
                bucket.IsTemporary))
            .ToList();
    }

    private FundingBucket CreateBucket(
        string name,
        Money monthlyTarget,
        BucketLifespan lifespan,
        MonthPeriod? onlyIn)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Bucket name is required.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(monthlyTarget);

        if (monthlyTarget.Equals(Money.Zero))
        {
            throw new ArgumentException("A bucket needs a monthly target.", nameof(monthlyTarget));
        }

        return new FundingBucket(
            Guid.NewGuid(),
            name.Trim(),
            monthlyTarget,
            _buckets.Count % FundingBucket.PaletteSize,
            lifespan,
            onlyIn);
    }

    private FundingBucket? Find(Guid id) =>
        _buckets.SingleOrDefault(bucket => bucket.Id == id);

    private Money FundedIn(Guid bucketId, MonthPeriod period) =>
        _funded.TryGetValue((bucketId, period.Year, period.Month), out var amount)
            ? amount
            : Money.Zero;
}
