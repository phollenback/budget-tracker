namespace BudgetTracker.Domain;

public sealed record BucketMonthFund(Guid BucketId, MonthPeriod Period, Money Amount);
