using BudgetTracker.Domain;

namespace BudgetTracker.Domain.Tests;

public class YearlySpendingTests
{
    [Fact]
    public void assemble_fills_all_twelve_months()
    {
        var overview = YearlySpending.Assemble(YearPeriod.From(2026), []);

        Assert.Equal(12, overview.Months.Count);
        Assert.All(overview.Months, month => Assert.False(month.HasWorksheet));
        Assert.Equal(Money.Zero, overview.TotalSpent);
        Assert.Equal(0, overview.SubmittedCount);
    }

    [Fact]
    public void assemble_sums_only_months_with_worksheet_data()
    {
        var june = MonthlyBudget.Open(MonthPeriod.From(2026, 6));
        june.SetIncome(Money.From(5000m));
        june.SetLineAmount("Mortgage/Rent", Money.From(1500m));
        june.SetLineAmount("Dining Out", Money.From(200m));
        june.Submit();

        var july = MonthlyBudget.Open(MonthPeriod.From(2026, 7));
        july.SetOwnerName("Ignored empty month");

        var overview = YearlySpending.Assemble(YearPeriod.From(2026), [june, july]);

        Assert.True(overview.Months[5].HasWorksheet);
        Assert.Equal("June", overview.Months[5].MonthName);
        Assert.Equal(Money.From(1700m), overview.Months[5].Spent);
        Assert.Equal(Money.From(5000m), overview.Months[5].Income);
        Assert.True(overview.Months[5].IsSubmitted);

        Assert.False(overview.Months[6].HasWorksheet);
        Assert.Equal(Money.From(1700m), overview.TotalSpent);
        Assert.Equal(Money.From(5000m), overview.TotalIncome);
        Assert.Equal(1, overview.SubmittedCount);
    }

    [Fact]
    public void assemble_ignores_budgets_from_other_years()
    {
        var otherYear = MonthlyBudget.Open(MonthPeriod.From(2025, 6));
        otherYear.SetIncome(Money.From(8000m));
        otherYear.SetLineAmount("Mortgage/Rent", Money.From(2000m));

        var overview = YearlySpending.Assemble(YearPeriod.From(2026), [otherYear]);

        Assert.Equal(Money.Zero, overview.TotalSpent);
        Assert.Equal(Money.Zero, overview.TotalIncome);
    }
}
