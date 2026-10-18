using BudgetTracker.Domain;

namespace BudgetTracker.Web.Mapping;

internal static class MoneyInputMapping
{
    public static Money ToMoney(decimal? amount)
    {
        if (amount is not > 0m)
        {
            return Money.Zero;
        }

        return Money.From(decimal.Round(amount.Value, 2, MidpointRounding.AwayFromZero));
    }

    public static decimal? ToInput(this Money money) =>
        money.Equals(Money.Zero) ? null : money.Amount;
}
