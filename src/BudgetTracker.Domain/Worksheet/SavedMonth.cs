namespace BudgetTracker.Domain;

public sealed record SavedMonth(MonthPeriod Period, bool IsSubmitted, DateTime? SubmittedAt);
