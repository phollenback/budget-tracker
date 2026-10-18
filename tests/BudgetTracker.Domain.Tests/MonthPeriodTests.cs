using BudgetTracker.Domain;

namespace BudgetTracker.Domain.Tests;

public class MonthPeriodTests
{
    [Fact]
    public void from_stores_the_first_of_the_month()
    {
        var period = MonthPeriod.From(2026, 6);

        Assert.Equal(2026, period.Year);
        Assert.Equal(6, period.Month);
        Assert.Equal(new DateOnly(2026, 6, 1), period.Start);
    }

    [Fact]
    public void from_date_ignores_the_day()
    {
        var period = MonthPeriod.From(new DateOnly(2026, 6, 22));

        Assert.Equal(MonthPeriod.From(2026, 6), period);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void from_rejects_invalid_months(int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MonthPeriod.From(2026, month));
    }

    [Fact]
    public void previous_of_january_is_december_of_prior_year()
    {
        Assert.Equal(MonthPeriod.From(2025, 12), MonthPeriod.From(2026, 1).Previous());
    }

    [Fact]
    public void next_of_december_is_january_of_next_year()
    {
        Assert.Equal(MonthPeriod.From(2027, 1), MonthPeriod.From(2026, 12).Next());
    }

    [Fact]
    public void june_2026_equals_june_2026()
    {
        Assert.Equal(MonthPeriod.From(2026, 6), MonthPeriod.From(2026, 6));
    }

    [Fact]
    public void to_string_is_month_and_year()
    {
        Assert.Equal("June 2026", MonthPeriod.From(2026, 6).ToString());
        Assert.Equal("June", MonthPeriod.From(2026, 6).MonthName);
    }
}
