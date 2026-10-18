using BudgetTracker.Domain;

namespace BudgetTracker.Domain.Tests;

public class YearPeriodTests
{
    [Fact]
    public void months_are_january_through_december()
    {
        var months = YearPeriod.From(2026).Months().ToList();

        Assert.Equal(12, months.Count);
        Assert.Equal(MonthPeriod.From(2026, 1), months[0]);
        Assert.Equal(MonthPeriod.From(2026, 12), months[11]);
    }

    [Fact]
    public void previous_and_next_move_one_year()
    {
        var year = YearPeriod.From(2026);

        Assert.Equal(2025, year.Previous().Year);
        Assert.Equal(2027, year.Next().Year);
    }
}
