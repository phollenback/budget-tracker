namespace BudgetTracker.Domain;

/// <summary>
/// A calendar year that owns twelve budget months.
/// </summary>
public sealed class YearPeriod : IEquatable<YearPeriod>
{
    public int Year { get; }

    private YearPeriod(int year) => Year = year;

    public static YearPeriod From(int year)
    {
        if (year is < 1 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1 and 9999.");
        }

        return new YearPeriod(year);
    }

    public YearPeriod Previous() => From(Year - 1);

    public YearPeriod Next() => From(Year + 1);

    public IEnumerable<MonthPeriod> Months()
    {
        for (var month = 1; month <= 12; month++)
        {
            yield return MonthPeriod.From(Year, month);
        }
    }

    public bool Equals(YearPeriod? other) => other is not null && Year == other.Year;

    public override bool Equals(object? obj) => Equals(obj as YearPeriod);

    public override int GetHashCode() => Year;

    public override string ToString() => Year.ToString();

    public static bool operator ==(YearPeriod? left, YearPeriod? right) =>
        ReferenceEquals(left, right) || left is not null && left.Equals(right);

    public static bool operator !=(YearPeriod? left, YearPeriod? right) => !(left == right);
}
