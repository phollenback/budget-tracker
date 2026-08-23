namespace BudgetTracker.Web.Models;

public sealed class BudgetFormSnapshot
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly? Date { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public decimal? NetMonthlyIncome { get; set; }
    public Dictionary<string, decimal?> Lines { get; set; } = new();

    public static BudgetFormSnapshot FromForm(BudgetForm form) =>
        new()
        {
            Year = form.Period.Year,
            Month = form.Period.Month,
            Name = form.Name,
            Date = form.Date,
            SubmittedAt = form.SubmittedAt,
            NetMonthlyIncome = form.NetMonthlyIncome,
            Lines = form.AllLines().ToDictionary(line => line.Name, line => line.Amount)
        };

    public BudgetForm ToForm()
    {
        var form = new BudgetForm
        {
            Period = new DateOnly(Year, Month, 1),
            Name = Name,
            Date = Date,
            SubmittedAt = SubmittedAt,
            NetMonthlyIncome = NetMonthlyIncome
        };

        foreach (var line in form.AllLines())
        {
            if (Lines.TryGetValue(line.Name, out var amount))
            {
                line.Amount = amount;
            }
        }

        return form;
    }
}
