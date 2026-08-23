namespace BudgetTracker.Web.Models;

public sealed record MonthWorksheetSummary(
    DateOnly Period,
    bool IsSubmitted,
    DateTime? SubmittedAt);
