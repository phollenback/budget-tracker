using BudgetTracker.Web.Models;

namespace BudgetTracker.Web.Services;

public interface IBudgetWorksheetStore
{
    Task<BudgetForm> GetOrCreateAsync(DateOnly period, CancellationToken cancellationToken = default);
    Task SaveAsync(BudgetForm form, bool submit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthWorksheetSummary>> ListAsync(CancellationToken cancellationToken = default);
}
