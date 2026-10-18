using BudgetTracker.Domain;

namespace BudgetTracker.Infrastructure;

internal sealed class BucketBookRecord
{
    public List<FundingBucketRecord> Buckets { get; set; } = [];
    public List<BucketFundRecord> Funds { get; set; } = [];

    public static BucketBookRecord FromBook(BucketBook book)
    {
        ArgumentNullException.ThrowIfNull(book);

        return new BucketBookRecord
        {
            Buckets = book.Buckets.Select(FundingBucketRecord.FromBucket).ToList(),
            Funds = book.Funds.Select(BucketFundRecord.FromFund).ToList()
        };
    }

    public BucketBook ToBook() =>
        BucketBook.Restore(
            Buckets.Select(record => record.ToBucket()),
            Funds.Select(record => record.ToFund()));
}

internal sealed class FundingBucketRecord
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Target { get; set; }
    public int ColorIndex { get; set; }
    public string Lifespan { get; set; } = nameof(BucketLifespan.Recurring);
    public int? Year { get; set; }
    public int? Month { get; set; }

    public static FundingBucketRecord FromBucket(FundingBucket bucket) =>
        new()
        {
            Id = bucket.Id,
            Name = bucket.Name,
            Target = bucket.MonthlyTarget.Amount,
            ColorIndex = bucket.ColorIndex,
            Lifespan = bucket.Lifespan.ToString(),
            Year = bucket.OnlyIn?.Year,
            Month = bucket.OnlyIn?.Month
        };

    public FundingBucket ToBucket()
    {
        var lifespan = Enum.TryParse<BucketLifespan>(Lifespan, out var parsed)
            ? parsed
            : BucketLifespan.Recurring;

        return FundingBucket.Restore(
            Id == Guid.Empty ? Guid.NewGuid() : Id,
            Name,
            Money.From(decimal.Round(Target, 2, MidpointRounding.AwayFromZero)),
            ColorIndex,
            lifespan,
            lifespan == BucketLifespan.ThisMonthOnly && Year is int year && Month is int month
                ? MonthPeriod.From(year, month)
                : null);
    }
}

internal sealed class BucketFundRecord
{
    public Guid BucketId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }

    public static BucketFundRecord FromFund(BucketMonthFund fund) =>
        new()
        {
            BucketId = fund.BucketId,
            Year = fund.Period.Year,
            Month = fund.Period.Month,
            Amount = fund.Amount.Amount
        };

    public BucketMonthFund ToFund() =>
        new(
            BucketId,
            MonthPeriod.From(Year, Month),
            Amount > 0m
                ? Money.From(decimal.Round(Amount, 2, MidpointRounding.AwayFromZero))
                : Money.Zero);
}
