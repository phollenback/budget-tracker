using BudgetTracker.Domain;

namespace BudgetTracker.Domain.Tests;

public class MonthlyBudgetTests
{
    [Fact]
    public void new_budget_has_every_official_line_at_zero()
    {
        var budget = OpenJune();

        var lines = budget.AllLines().ToList();

        Assert.Equal(36, lines.Count);
        Assert.All(lines, line => Assert.Equal(Money.Zero, line.Amount));
        Assert.Equal(Money.Zero, budget.NetMonthlyIncome);
        Assert.Equal(BudgetStatus.Draft, budget.Status);
    }

    [Fact]
    public void groceries_are_essential_not_discretionary()
    {
        var budget = OpenJune();
        budget.SetLineAmount("Groceries", Money.From(400m));

        Assert.Equal(Money.From(400m), budget.EssentialSubtotal);
        Assert.Equal(Money.Zero, budget.DiscretionarySubtotal);
        Assert.Equal("Household / Personal", budget.CategoryNamed("Household / Personal")!.Name);
        Assert.Equal(AllocationBucket.Essential, budget.CategoryNamed("Household / Personal")!.Bucket);
    }

    [Fact]
    public void setting_rent_increases_housing_and_essential_subtotals()
    {
        var budget = OpenJune();
        budget.SetLineAmount("Mortgage/Rent", Money.From(1500m));

        Assert.Equal(Money.From(1500m), budget.CategoryNamed("Housing")!.Subtotal);
        Assert.Equal(Money.From(1500m), budget.EssentialSubtotal);
        Assert.Equal(Money.From(1500m), budget.TotalMonthlyExpenses);
        Assert.Equal(Money.From(1500m), budget.LessExpenses);
    }

    [Fact]
    public void five_thousand_income_with_sixty_twenty_twenty_is_balanced()
    {
        var budget = OpenJune();
        budget.SetIncome(Money.From(5000m));
        budget.SetLineAmount("Mortgage/Rent", Money.From(3000m));
        budget.SetLineAmount("Dining Out", Money.From(1000m));
        budget.SetLineAmount("Emergency Fund", Money.From(1000m));

        var snapshot = budget.CalculateAllocation();

        Assert.Equal(Percentage.FromPoints(60m), snapshot.Essential.ShareOfIncome);
        Assert.Equal(Percentage.FromPoints(20m), snapshot.Discretionary.ShareOfIncome);
        Assert.Equal(Percentage.FromPoints(20m), snapshot.SavingAndInvesting.ShareOfIncome);
        Assert.Equal(0m, snapshot.Essential.VariancePoints);
        Assert.Equal(Money.From(4000m), snapshot.TotalMonthlyExpenses);
        Assert.True(snapshot.SurplusOrDeficit.IsBalanced);
    }

    [Fact]
    public void essential_over_sixty_percent_reports_positive_variance()
    {
        var budget = OpenJune();
        budget.SetIncome(Money.From(5000m));
        budget.SetLineAmount("Mortgage/Rent", Money.From(3500m));

        var snapshot = budget.CalculateAllocation();

        Assert.Equal(Percentage.FromPoints(70m), snapshot.Essential.ShareOfIncome);
        Assert.Equal(10m, snapshot.Essential.VariancePoints);
    }

    [Fact]
    public void surplus_subtracts_savings()
    {
        var budget = OpenJune();
        budget.SetIncome(Money.From(5000m));
        budget.SetLineAmount("Mortgage/Rent", Money.From(2000m));
        budget.SetLineAmount("Dining Out", Money.From(500m));
        budget.SetLineAmount("Retirement Savings", Money.From(1500m));

        Assert.Equal(1000m, budget.SurplusOrDeficit.Amount);
        Assert.True(budget.SurplusOrDeficit.IsSurplus);
        Assert.Equal(Money.From(2500m), budget.TotalMonthlyExpenses);
    }

    [Fact]
    public void zero_income_does_not_throw_when_calculating_allocation()
    {
        var budget = OpenJune();
        budget.SetLineAmount("Gas", Money.From(40m));

        var snapshot = budget.CalculateAllocation();

        Assert.Null(snapshot.Essential.ShareOfIncome);
        Assert.Null(snapshot.Essential.VariancePoints);
        Assert.True(snapshot.SurplusOrDeficit.IsDeficit);
    }

    [Fact]
    public void cannot_set_amount_on_unknown_line()
    {
        var budget = OpenJune();

        Assert.Throws<ArgumentException>(() => budget.SetLineAmount("Yacht", Money.From(1m)));
    }

    [Fact]
    public void submit_marks_budget_submitted()
    {
        var budget = OpenJune();

        budget.Submit();

        Assert.True(budget.IsSubmitted);
        Assert.Equal(BudgetStatus.Submitted, budget.Status);
        Assert.NotNull(budget.SubmittedAt);
        Assert.NotNull(budget.CompletedOn);
    }

    [Fact]
    public void official_chart_includes_long_term_care_insurance()
    {
        var budget = OpenJune();

        budget.SetLineAmount("Long-term Care Insurance", Money.From(50m));

        Assert.Equal(Money.From(50m), budget.AllLines().Single(line => line.Name == "Long-term Care Insurance").Amount);
    }

    private static MonthlyBudget OpenJune() => MonthlyBudget.Open(MonthPeriod.From(2026, 6));
}
