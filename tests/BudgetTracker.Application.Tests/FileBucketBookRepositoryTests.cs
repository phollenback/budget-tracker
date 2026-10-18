using BudgetTracker.Domain;
using BudgetTracker.Infrastructure;

namespace BudgetTracker.Application.Tests;

public class FileBucketBookRepositoryTests
{
    [Fact]
    public async Task round_trips_recurring_and_temporary_fills()
    {
        using var directory = new TempDirectory();
        var repository = new FileBucketBookRepository(directory.Path);
        var book = BucketBook.Empty();
        var emergency = book.AddRecurring("Emergency fund", Money.From(400m));
        var gifts = book.AddTemporary(MonthPeriod.From(2026, 12), "Gifts", Money.From(150m));
        book.Contribute(emergency.Id, MonthPeriod.From(2026, 8), Money.From(40m));
        book.Contribute(gifts.Id, MonthPeriod.From(2026, 12), Money.From(25m));

        await repository.SaveAsync(book);
        var loaded = await repository.GetAsync();

        var august = loaded.BoardFor(MonthPeriod.From(2026, 8)).Single();
        var december = loaded.BoardFor(MonthPeriod.From(2026, 12));

        Assert.Equal("Emergency fund", august.Name);
        Assert.Equal(Money.From(40m), august.Funded);
        Assert.Equal(2, december.Count);
        Assert.Equal(Money.From(25m), december.Single(fill => fill.IsTemporary).Funded);
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "budget-tracker-bucket-tests",
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
