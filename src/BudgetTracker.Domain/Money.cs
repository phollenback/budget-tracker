namespace BudgetTracker.Domain;

/// <summary>
/// Non-negative USD amount with at most 2 decimal places. Equality is by value.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public const string Usd = "USD";

    public static Money Zero { get; } = new(0m, Usd);

    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money From(decimal amount)
    {
        if (amount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Money cannot be negative.");
        }

        if (decimal.Round(amount, 2, MidpointRounding.AwayFromZero) != amount)
        {
            throw new ArgumentException("Money must have at most 2 decimal places.", nameof(amount));
        }

        return new Money(amount, Usd);
    }

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);

        if (other.Amount > Amount)
        {
            throw new InvalidOperationException("Money cannot be negative.");
        }

        return new Money(Amount - other.Amount, Currency);
    }

    public static Money Sum(IEnumerable<Money> amounts)
    {
        ArgumentNullException.ThrowIfNull(amounts);
        return amounts.Aggregate(Zero, (total, amount) => total.Add(amount));
    }

    public bool Equals(Money? other) =>
        other is not null && Amount == other.Amount && Currency == other.Currency;

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() =>
        Amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));

    public static bool operator ==(Money? left, Money? right) =>
        ReferenceEquals(left, right) || left is not null && left.Equals(right);

    public static bool operator !=(Money? left, Money? right) => !(left == right);

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot combine {Currency} with {other.Currency}.");
        }
    }
}
