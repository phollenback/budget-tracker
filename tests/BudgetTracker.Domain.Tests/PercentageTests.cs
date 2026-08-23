using BudgetTracker.Domain;

namespace BudgetTracker.Domain.Tests;

public class PercentageTests
{
    [Fact]
    public void share_of_income_is_sixty_when_3000_of_5000()
    {
        var share = Percentage.ShareOf(Money.From(3000m), Money.From(5000m));

        Assert.Equal(Percentage.FromPoints(60m), share);
    }

    [Fact]
    public void share_of_zero_income_is_null()
    {
        Assert.Null(Percentage.ShareOf(Money.From(100m), Money.Zero));
    }

    [Fact]
    public void from_points_rejects_negatives()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Percentage.FromPoints(-1m));
    }
}
