using BudgetTracker.Domain;

namespace BudgetTracker.Infrastructure;

internal sealed class MonthlyBudgetRecord
{
    public Guid? Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly? Date { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public decimal? NetMonthlyIncome { get; set; }
    public Dictionary<string, decimal?> Lines { get; set; } = new();

    public static MonthlyBudgetRecord FromBudget(MonthlyBudget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);

        return new MonthlyBudgetRecord
        {
            Id = budget.Id,
            Year = budget.Period.Year,
            Month = budget.Period.Month,
            Name = budget.OwnerName,
            Date = budget.CompletedOn,
            SubmittedAt = budget.SubmittedAt,
            NetMonthlyIncome = budget.NetMonthlyIncome.Equals(Money.Zero) ? null : budget.NetMonthlyIncome.Amount,
            Lines = budget.AllLines().ToDictionary(
                line => line.Name,
                line => line.Amount.Equals(Money.Zero) ? (decimal?)null : line.Amount.Amount)
        };
    }

    public MonthlyBudget ToBudget()
    {
        var period = MonthPeriod.From(Year, Month);
        var amounts = new Dictionary<string, Money>();
        foreach (var (name, amount) in Lines)
        {
            if (amount is > 0m)
            {
                amounts[name] = Money.From(decimal.Round(amount.Value, 2, MidpointRounding.AwayFromZero));
            }
        }

        return MonthlyBudget.Restore(
            Id ?? DeterministicId(period),
            period,
            Name,
            Date,
            NetMonthlyIncome is > 0m
                ? Money.From(decimal.Round(NetMonthlyIncome.Value, 2, MidpointRounding.AwayFromZero))
                : Money.Zero,
            SubmittedAt.HasValue ? BudgetStatus.Submitted : BudgetStatus.Draft,
            SubmittedAt,
            amounts);
    }

    private static Guid DeterministicId(MonthPeriod period)
    {
        var bytes = new byte[16];
        BitConverter.TryWriteBytes(bytes.AsSpan(0, 4), period.Year);
        BitConverter.TryWriteBytes(bytes.AsSpan(4, 4), period.Month);
        return new Guid(bytes);
    }
}
