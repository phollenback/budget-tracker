using BudgetTracker.Application;
using BudgetTracker.Domain;

namespace BudgetTracker.Application.Tests;

public class BudgetWorkspaceTests
{
    [Fact]
    public async Task get_or_open_copies_the_latest_owner_name()
    {
        var repository = new InMemoryMonthlyBudgetRepository();
        var june = MonthlyBudget.Open(MonthPeriod.From(2026, 6));
        june.SetOwnerName("Peyton");
        june.SetIncome(Money.From(100m));
        await repository.SaveAsync(june);

        var workspace = new BudgetWorkspace(repository, new InMemoryBucketBookRepository());
        var july = await workspace.GetOrOpenAsync(MonthPeriod.From(2026, 7));

        Assert.Equal("Peyton", july.OwnerName);
        Assert.False(july.HasWorksheetData);
    }

    [Fact]
    public async Task get_year_is_assembled_from_saved_months()
    {
        var repository = new InMemoryMonthlyBudgetRepository();
        var june = MonthlyBudget.Open(MonthPeriod.From(2026, 6));
        june.SetIncome(Money.From(4000m));
        june.SetLineAmount("Groceries", Money.From(300m));
        await repository.SaveAsync(june);

        var workspace = new BudgetWorkspace(repository, new InMemoryBucketBookRepository());
        var overview = await workspace.GetYearAsync(YearPeriod.From(2026));

        Assert.Equal(Money.From(300m), overview.TotalSpent);
        Assert.Equal(Money.From(4000m), overview.TotalIncome);
        Assert.True(overview.Months[5].HasWorksheet);
        Assert.False(overview.Months[0].HasWorksheet);
    }

    [Fact]
    public async Task get_bucket_fill_returns_empty_when_month_has_no_worksheet()
    {
        var workspace = new BudgetWorkspace(new InMemoryMonthlyBudgetRepository(), new InMemoryBucketBookRepository());

        var board = await workspace.GetBucketFillAsync(MonthPeriod.From(2026, 8));

        Assert.False(board.HasWorksheet);
        Assert.Empty(board.Buckets);
    }

    [Fact]
    public async Task submit_persists_the_aggregate_as_submitted()
    {
        var repository = new InMemoryMonthlyBudgetRepository();
        var workspace = new BudgetWorkspace(repository, new InMemoryBucketBookRepository());
        var budget = await workspace.GetOrOpenAsync(MonthPeriod.From(2026, 6));
        budget.SetIncome(Money.From(2500m));

        await workspace.SubmitAsync(budget);

        var loaded = await repository.GetAsync(MonthPeriod.From(2026, 6));
        Assert.NotNull(loaded);
        Assert.True(loaded.IsSubmitted);
        Assert.Equal(Money.From(2500m), loaded.NetMonthlyIncome);
    }
}
