using BudgetTracker.Domain;
using BudgetTracker.Infrastructure;

namespace BudgetTracker.Application.Tests;

public class FileMonthlyBudgetRepositoryTests
{
    [Fact]
    public async Task round_trips_a_submitted_month()
    {
        using var directory = new TempDirectory();
        var repository = new FileMonthlyBudgetRepository(directory.Path);
        var budget = MonthlyBudget.Open(MonthPeriod.From(2026, 6));
        budget.SetOwnerName("Peyton");
        budget.SetIncome(Money.From(5000m));
        budget.SetLineAmount("Mortgage/Rent", Money.From(1800m));
        budget.Submit();

        await repository.SaveAsync(budget);

        var loaded = await repository.GetAsync(MonthPeriod.From(2026, 6));

        Assert.NotNull(loaded);
        Assert.Equal(budget.Id, loaded.Id);
        Assert.Equal("Peyton", loaded.OwnerName);
        Assert.True(loaded.IsSubmitted);
        Assert.Equal(Money.From(1800m), loaded.EssentialSubtotal);
    }

    [Fact]
    public async Task list_for_year_does_not_include_other_years()
    {
        using var directory = new TempDirectory();
        var repository = new FileMonthlyBudgetRepository(directory.Path);

        var june = MonthlyBudget.Open(MonthPeriod.From(2026, 6));
        june.SetIncome(Money.From(100m));
        await repository.SaveAsync(june);

        var prior = MonthlyBudget.Open(MonthPeriod.From(2025, 6));
        prior.SetIncome(Money.From(900m));
        await repository.SaveAsync(prior);

        var year = await repository.ListForYearAsync(YearPeriod.From(2026));

        Assert.Single(year);
        Assert.Equal(2026, year[0].Period.Year);
    }

    [Fact]
    public async Task list_saved_skips_name_only_files_and_reads_legacy_json()
    {
        using var directory = new TempDirectory();
        await File.WriteAllTextAsync(Path.Combine(directory.Path, "2026-03-01.json"), """
            {
              "Year": 2026,
              "Month": 3,
              "Name": "Peyton",
              "Date": null,
              "SubmittedAt": null,
              "NetMonthlyIncome": null,
              "Lines": {}
            }
            """);
        await File.WriteAllTextAsync(Path.Combine(directory.Path, "2026-04-01.json"), """
            {
              "Year": 2026,
              "Month": 4,
              "Name": "Peyton",
              "Date": "2026-04-02",
              "SubmittedAt": "2026-04-02T12:00:00",
              "NetMonthlyIncome": 4200.00,
              "Lines": {
                "Groceries": 350.00
              }
            }
            """);

        var repository = new FileMonthlyBudgetRepository(directory.Path);
        var saved = await repository.ListSavedAsync();
        var april = await repository.GetAsync(MonthPeriod.From(2026, 4));

        Assert.Single(saved);
        Assert.Equal(4, saved[0].Period.Month);
        Assert.True(saved[0].IsSubmitted);
        Assert.NotNull(april);
        Assert.Equal(Money.From(350m), april.EssentialSubtotal);
        Assert.Equal("Peyton", april.OwnerName);
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "budget-tracker-tests",
            Guid.NewGuid().ToString("N"));

        public TempDirectory() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
