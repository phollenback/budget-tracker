using BudgetTracker.Domain;

namespace BudgetTracker.Domain.Tests;

public class BalanceTests
{
    [Fact]
    public void surplus_subtracts_savings_not_only_expenses()
    {
        var balance = Balance.Of(
            Money.From(5000m),
            essential: Money.From(3000m),
            discretionary: Money.From(1000m),
            saving: Money.From(1000m));

        Assert.True(balance.IsBalanced);
        Assert.Equal(0m, balance.Amount);
    }

    [Fact]
    public void overspending_is_a_deficit()
    {
        var balance = Balance.Of(
            Money.From(1000m),
            essential: Money.From(800m),
            discretionary: Money.From(300m),
            saving: Money.Zero);

        Assert.True(balance.IsDeficit);
        Assert.Equal(-100m, balance.Amount);
    }
}
