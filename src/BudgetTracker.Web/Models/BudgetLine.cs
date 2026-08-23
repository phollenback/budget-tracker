namespace BudgetTracker.Web.Models;

public sealed class BudgetLine
{
    public string Name { get; init; } = string.Empty;
    public decimal? Amount { get; set; }
}
