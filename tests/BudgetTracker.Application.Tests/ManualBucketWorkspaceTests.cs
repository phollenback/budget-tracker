using BudgetTracker.Application;
using BudgetTracker.Domain;

namespace BudgetTracker.Application.Tests;

public class ManualBucketWorkspaceTests
{
    [Fact]
    public async Task recurring_buckets_carry_across_months_with_separate_fills()
    {
        var workspace = new BudgetWorkspace(new InMemoryMonthlyBudgetRepository(), new InMemoryBucketBookRepository());
        await workspace.AddRecurringBucketAsync("Emergency fund", Money.From(400m));

        var june = MonthPeriod.From(2026, 6);
        var july = MonthPeriod.From(2026, 7);
        var bucket = (await workspace.GetManualBucketsAsync(june)).Single();
        await workspace.ContributeToBucketAsync(bucket.Id, june, Money.From(80m));

        var juneBoard = await workspace.GetManualBucketsAsync(june);
        var julyBoard = await workspace.GetManualBucketsAsync(july);

        Assert.Equal(Money.From(80m), juneBoard.Single().Funded);
        Assert.Equal(Money.Zero, julyBoard.Single().Funded);
        Assert.Equal("Emergency fund", julyBoard.Single().Name);
    }

    [Fact]
    public async Task temporary_bucket_is_only_on_the_month_it_was_added()
    {
        var workspace = new BudgetWorkspace(new InMemoryMonthlyBudgetRepository(), new InMemoryBucketBookRepository());
        var june = MonthPeriod.From(2026, 6);
        await workspace.AddTemporaryBucketAsync(june, "Concert tickets", Money.From(90m));

        Assert.Single(await workspace.GetManualBucketsAsync(june));
        Assert.Empty(await workspace.GetManualBucketsAsync(MonthPeriod.From(2026, 7)));
    }
}
