using BudgetTracker.Domain;

namespace BudgetTracker.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void from_accepts_two_decimal_places()
    {
        var money = Money.From(1500.25m);

        Assert.Equal(1500.25m, money.Amount);
        Assert.Equal(Money.Usd, money.Currency);
    }

    [Fact]
    public void from_rejects_negative_amounts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Money.From(-0.01m));
    }

    [Fact]
    public void from_rejects_more_than_two_decimal_places()
    {
        Assert.Throws<ArgumentException>(() => Money.From(1.234m));
    }

    [Fact]
    public void add_sums_two_amounts()
    {
        Assert.Equal(Money.From(7.50m), Money.From(3m).Add(Money.From(4.50m)));
    }

    [Fact]
    public void subtract_rejects_a_result_below_zero()
    {
        Assert.Throws<InvalidOperationException>(() => Money.From(1m).Subtract(Money.From(2m)));
    }

    [Fact]
    public void sum_of_empty_list_is_zero()
    {
        Assert.Equal(Money.Zero, Money.Sum([]));
    }

    [Fact]
    public void equal_amounts_are_equal()
    {
        Assert.Equal(Money.From(100m), Money.From(100.00m));
        Assert.True(Money.From(100m) == Money.From(100.00m));
    }

    [Fact]
    public void to_string_uses_usd_currency_format()
    {
        Assert.Equal("$1,500.00", Money.From(1500m).ToString());
    }
}
