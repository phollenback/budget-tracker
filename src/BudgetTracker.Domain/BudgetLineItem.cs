namespace BudgetTracker.Domain;

public sealed class BudgetLineItem
{
    public string Name { get; }
    public Money Amount { get; private set; }

    internal BudgetLineItem(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Line item name is required.", nameof(name));
        }

        Name = name;
        Amount = Money.Zero;
    }

    internal void SetAmount(Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        Amount = amount;
    }
}
