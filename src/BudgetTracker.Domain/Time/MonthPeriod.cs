using System.Globalization;

namespace BudgetTracker.Domain;

/// <summary>
/// A budget month (June 2026). The underlying date is always the 1st.
/// </summary>
public sealed class MonthPeriod : IEquatable<MonthPeriod>
{
    public int Year { get; }
    public int Month { get; }
    public DateOnly Start { get; }

    private MonthPeriod(int year, int month)
    {
        Year = year;
        Month = month;
        Start = new DateOnly(year, month, 1);
    }

    public static MonthPeriod From(int year, int month)
    {
        if (year is < 1 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1 and 9999.");
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        return new MonthPeriod(year, month);
    }

    public static MonthPeriod From(DateOnly date) => From(date.Year, date.Month);

    public string MonthName => Start.ToString("MMMM", CultureInfo.InvariantCulture);

    public MonthPeriod Previous() => From(Start.AddMonths(-1));

    public MonthPeriod Next() => From(Start.AddMonths(1));

    public bool Equals(MonthPeriod? other) =>
        other is not null && Year == other.Year && Month == other.Month;

    public override bool Equals(object? obj) => Equals(obj as MonthPeriod);

    public override int GetHashCode() => HashCode.Combine(Year, Month);

    public override string ToString() => Start.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

    public static bool operator ==(MonthPeriod? left, MonthPeriod? right) =>
        ReferenceEquals(left, right) || left is not null && left.Equals(right);

    public static bool operator !=(MonthPeriod? left, MonthPeriod? right) => !(left == right);
}
