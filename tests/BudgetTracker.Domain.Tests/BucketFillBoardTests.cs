using BudgetTracker.Domain;

namespace BudgetTracker.Domain.Tests;

public class BucketFillBoardTests
{
    [Fact]
    public void sixty_twenty_twenty_marks_every_bucket_filled()
    {
        var budget = MonthlyBudget.Open(MonthPeriod.From(2026, 6));
        budget.SetIncome(Money.From(5000m));
        budget.SetLineAmount("Mortgage/Rent", Money.From(3000m));
        budget.SetLineAmount("Dining Out", Money.From(1000m));
        budget.SetLineAmount("Emergency Fund", Money.From(1000m));

        var board = BucketFillBoard.From(budget);

        Assert.True(board.HasWorksheet);
        Assert.All(board.Buckets, bucket => Assert.True(bucket.IsFilled));
        Assert.Equal(100, board.Buckets.Single(bucket => bucket.Bucket == AllocationBucket.Essential).ProgressPercent);
        Assert.True(board.SavingsGoals.Single(goal => goal.Name == "Emergency Fund").IsFilled);
        Assert.False(board.SavingsGoals.Single(goal => goal.Name == "College Savings").IsFilled);
    }

    [Fact]
    public void essential_under_target_is_not_filled()
    {
        var budget = MonthlyBudget.Open(MonthPeriod.From(2026, 6));
        budget.SetIncome(Money.From(5000m));
        budget.SetLineAmount("Mortgage/Rent", Money.From(2000m));

        var essential = BucketFillBoard.From(budget).Buckets
            .Single(bucket => bucket.Bucket == AllocationBucket.Essential);

        Assert.False(essential.IsFilled);
        Assert.Equal(Money.From(3000m), essential.TargetAmount);
        Assert.Equal(Money.From(2000m), essential.Actual);
        Assert.Equal(67, essential.ProgressPercent);
    }

    [Fact]
    public void zero_income_with_spend_still_counts_as_filled()
    {
        var budget = MonthlyBudget.Open(MonthPeriod.From(2026, 6));
        budget.SetLineAmount("Gas", Money.From(40m));

        var essential = BucketFillBoard.From(budget).Buckets
            .Single(bucket => bucket.Bucket == AllocationBucket.Essential);

        Assert.True(essential.IsFilled);
        Assert.Equal(100, essential.ProgressPercent);
        Assert.Equal(Money.Zero, essential.TargetAmount);
    }

    [Fact]
    public void empty_board_has_no_buckets()
    {
        var board = BucketFillBoard.Empty(MonthPeriod.From(2026, 8));

        Assert.False(board.HasWorksheet);
        Assert.Empty(board.Buckets);
        Assert.Empty(board.SavingsGoals);
    }
}
