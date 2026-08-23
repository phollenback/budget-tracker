namespace BudgetTracker.Domain;

/// <summary>
/// A percentage in points (60 means 60%). Actual share of income may exceed 100.
/// </summary>
public sealed class Percentage : IEquatable<Percentage>
{
    public decimal Points { get; }

    private Percentage(decimal points)
    {
        Points = points;
    }

    public static Percentage FromPoints(decimal points)
    {
        if (points < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(points), "Percentage cannot be negative.");
        }

        if (decimal.Round(points, 2, MidpointRounding.AwayFromZero) != points)
        {
            throw new ArgumentException("Percentage must have at most 2 decimal places.", nameof(points));
        }

        return new Percentage(points);
    }

    /// <summary>
    /// Returns null when <paramref name="whole"/> is zero so callers do not divide by zero.
    /// </summary>
    public static Percentage? ShareOf(Money part, Money whole)
    {
        ArgumentNullException.ThrowIfNull(part);
        ArgumentNullException.ThrowIfNull(whole);

        if (whole.Equals(Money.Zero))
        {
            return null;
        }

        var points = decimal.Round(part.Amount / whole.Amount * 100m, 2, MidpointRounding.AwayFromZero);
        return FromPoints(points);
    }

    public bool Equals(Percentage? other) => other is not null && Points == other.Points;

    public override bool Equals(object? obj) => Equals(obj as Percentage);

    public override int GetHashCode() => Points.GetHashCode();

    public override string ToString() => $"{Points:0.##}%";

    public static bool operator ==(Percentage? left, Percentage? right) =>
        ReferenceEquals(left, right) || left is not null && left.Equals(right);

    public static bool operator !=(Percentage? left, Percentage? right) => !(left == right);
}
