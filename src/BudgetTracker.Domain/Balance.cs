namespace BudgetTracker.Domain;

/// <summary>
/// Signed leftover after essential, discretionary, and planned savings.
/// Negative is a deficit.
/// </summary>
public sealed class Balance : IEquatable<Balance>
{
    public decimal Amount { get; }

    public bool IsSurplus => Amount > 0m;
    public bool IsDeficit => Amount < 0m;
    public bool IsBalanced => Amount == 0m;

    private Balance(decimal amount)
    {
        Amount = amount;
    }

    public static Balance Of(Money income, Money essential, Money discretionary, Money saving)
    {
        ArgumentNullException.ThrowIfNull(income);
        ArgumentNullException.ThrowIfNull(essential);
        ArgumentNullException.ThrowIfNull(discretionary);
        ArgumentNullException.ThrowIfNull(saving);

        return new Balance(income.Amount - essential.Amount - discretionary.Amount - saving.Amount);
    }

    public bool Equals(Balance? other) => other is not null && Amount == other.Amount;

    public override bool Equals(object? obj) => Equals(obj as Balance);

    public override int GetHashCode() => Amount.GetHashCode();

    public override string ToString() =>
        Amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));

    public static bool operator ==(Balance? left, Balance? right) =>
        ReferenceEquals(left, right) || left is not null && left.Equals(right);

    public static bool operator !=(Balance? left, Balance? right) => !(left == right);
}
