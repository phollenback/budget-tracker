namespace BudgetTracker.Domain;

public sealed class SavingsGoalFill
{
    public string Name { get; }
    public Money Amount { get; }
    public bool IsFilled => !Amount.Equals(Money.Zero);

    private SavingsGoalFill(string name, Money amount)
    {
        Name = name;
        Amount = amount;
    }

    public static SavingsGoalFill From(BudgetLineItem line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return new SavingsGoalFill(line.Name, line.Amount);
    }
}
