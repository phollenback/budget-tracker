using BudgetTracker.Domain;

namespace BudgetTracker.Web.Models;

public static class BudgetFormMapping
{
    public static MonthlyBudget ToDomain(this BudgetForm form)
    {
        ArgumentNullException.ThrowIfNull(form);

        var budget = MonthlyBudget.Open(MonthPeriod.From(form.Period));
        budget.SetOwnerName(form.Name);
        budget.SetCompletedOn(form.Date);

        if (form.NetMonthlyIncome.HasValue)
        {
            budget.SetIncome(Money.From(form.NetMonthlyIncome.Value));
        }

        foreach (var line in form.AllLines())
        {
            if (line.Amount.HasValue)
            {
                budget.SetLineAmount(line.Name, Money.From(line.Amount.Value));
            }
        }

        if (form.IsSubmitted)
        {
            budget.Submit();
        }

        return budget;
    }
}
