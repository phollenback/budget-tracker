using BudgetTracker.Domain;

namespace BudgetTracker.Domain.Tests;

public class BucketBookTests
{
    [Fact]
    public void recurring_bucket_appears_every_month_with_a_fresh_fill()
    {
        var book = BucketBook.Empty();
        var emergency = book.AddRecurring("Emergency fund", Money.From(400m));
        book.Contribute(emergency.Id, MonthPeriod.From(2026, 6), Money.From(150m));

        var june = book.BoardFor(MonthPeriod.From(2026, 6)).Single();
        var july = book.BoardFor(MonthPeriod.From(2026, 7)).Single();

        Assert.Equal("Emergency fund", july.Name);
        Assert.Equal(Money.From(150m), june.Funded);
        Assert.Equal(Money.Zero, july.Funded);
        Assert.False(july.IsTemporary);
    }

    [Fact]
    public void contributing_adds_to_what_is_already_in_the_bucket()
    {
        var book = BucketBook.Empty();
        var vacation = book.AddRecurring("Vacation", Money.From(300m));
        var june = MonthPeriod.From(2026, 6);

        book.Contribute(vacation.Id, june, Money.From(50m));
        book.Contribute(vacation.Id, june, Money.From(25m));

        var fill = book.BoardFor(june).Single();
        Assert.Equal(Money.From(75m), fill.Funded);
        Assert.Equal(25, fill.ProgressPercent);
        Assert.False(fill.IsFilled);
    }

    [Fact]
    public void temporary_bucket_only_appears_in_its_month()
    {
        var book = BucketBook.Empty();
        book.AddRecurring("Groceries", Money.From(500m));
        var gifts = book.AddTemporary(MonthPeriod.From(2026, 12), "Holiday gifts", Money.From(200m));
        book.Contribute(gifts.Id, MonthPeriod.From(2026, 12), Money.From(200m));

        var december = book.BoardFor(MonthPeriod.From(2026, 12));
        var november = book.BoardFor(MonthPeriod.From(2026, 11));

        Assert.Equal(2, december.Count);
        Assert.Contains(december, fill => fill.Name == "Holiday gifts" && fill.IsTemporary && fill.IsFilled);
        Assert.Single(november);
        Assert.Equal("Groceries", november[0].Name);
    }

    [Fact]
    public void cannot_contribute_to_a_temporary_bucket_in_another_month()
    {
        var book = BucketBook.Empty();
        var gifts = book.AddTemporary(MonthPeriod.From(2026, 12), "Holiday gifts", Money.From(200m));

        var error = Assert.Throws<InvalidOperationException>(
            () => book.Contribute(gifts.Id, MonthPeriod.From(2026, 11), Money.From(20m)));

        Assert.Contains("November 2026", error.Message);
    }

    [Fact]
    public void recurring_bucket_requires_a_name_and_target()
    {
        var book = BucketBook.Empty();

        Assert.Throws<ArgumentException>(() => book.AddRecurring("  ", Money.From(10m)));
        Assert.Throws<ArgumentException>(() => book.AddRecurring("Car", Money.Zero));
    }
}
