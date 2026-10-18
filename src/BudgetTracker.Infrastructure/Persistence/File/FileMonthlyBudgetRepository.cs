using System.Text.Json;
using BudgetTracker.Domain;

namespace BudgetTracker.Infrastructure;

public sealed class FileMonthlyBudgetRepository : IMonthlyBudgetRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _directory;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IReadOnlyList<SavedMonth>? _index;

    public FileMonthlyBudgetRepository(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _directory = directory;
    }

    public async Task<MonthlyBudget?> GetAsync(MonthPeriod period, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(period);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await ReadFromDiskAsync(period, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(MonthlyBudget budget, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(budget);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_directory);

            var record = MonthlyBudgetRecord.FromBudget(budget);
            var json = JsonSerializer.Serialize(record, JsonOptions);
            await File.WriteAllTextAsync(PathFor(budget.Period), json, cancellationToken);
            _index = null;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<MonthlyBudget>> ListForYearAsync(
        YearPeriod year,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(year);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var budgets = new List<MonthlyBudget>();
            foreach (var period in year.Months())
            {
                var budget = await ReadFromDiskAsync(period, cancellationToken);
                if (budget is not null)
                {
                    budgets.Add(budget);
                }
            }

            return budgets;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<SavedMonth>> ListSavedAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_index is not null)
            {
                return _index;
            }

            Directory.CreateDirectory(_directory);

            var saved = new List<SavedMonth>();
            foreach (var path in Directory.EnumerateFiles(_directory, "*-*-01.json"))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entry = await PeekSavedMonthAsync(path, cancellationToken);
                if (entry is not null)
                {
                    saved.Add(entry);
                }
            }

            _index = saved
                .OrderByDescending(month => month.Period.Start)
                .ToList();

            return _index;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<string?> LatestOwnerNameAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_directory);

            foreach (var path in Directory.EnumerateFiles(_directory, "*-*-01.json").OrderByDescending(name => name))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await using var stream = File.OpenRead(path);
                using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                if (document.RootElement.TryGetProperty("Name", out var name)
                    && name.ValueKind == JsonValueKind.String)
                {
                    var owner = name.GetString();
                    if (!string.IsNullOrWhiteSpace(owner))
                    {
                        return owner.Trim();
                    }
                }
            }

            return null;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<MonthlyBudget?> ReadFromDiskAsync(MonthPeriod period, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_directory);
        var path = PathFor(period);
        if (!File.Exists(path))
        {
            return null;
        }

        return await ReadBudgetAsync(path, cancellationToken);
    }

    private static async Task<MonthlyBudget> ReadBudgetAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var record = await JsonSerializer.DeserializeAsync<MonthlyBudgetRecord>(stream, JsonOptions, cancellationToken)
                     ?? throw new InvalidOperationException($"Could not read worksheet {path}.");
        return record.ToBudget();
    }

    private static async Task<SavedMonth?> PeekSavedMonthAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        var year = root.GetProperty("Year").GetInt32();
        var month = root.GetProperty("Month").GetInt32();
        DateTime? submittedAt = null;
        if (root.TryGetProperty("SubmittedAt", out var submitted) && submitted.ValueKind != JsonValueKind.Null)
        {
            submittedAt = submitted.GetDateTime();
        }

        var hasDate = root.TryGetProperty("Date", out var date) && date.ValueKind != JsonValueKind.Null;
        var hasIncome = root.TryGetProperty("NetMonthlyIncome", out var income)
                        && income.ValueKind is JsonValueKind.Number
                        && income.GetDecimal() != 0m;
        var hasLines = false;
        if (root.TryGetProperty("Lines", out var lines) && lines.ValueKind == JsonValueKind.Object)
        {
            foreach (var line in lines.EnumerateObject())
            {
                if (line.Value.ValueKind == JsonValueKind.Number && line.Value.GetDecimal() != 0m)
                {
                    hasLines = true;
                    break;
                }
            }
        }

        var isSubmitted = submittedAt.HasValue;
        if (!isSubmitted && !hasDate && !hasIncome && !hasLines)
        {
            return null;
        }

        return new SavedMonth(MonthPeriod.From(year, month), isSubmitted, submittedAt);
    }

    private string PathFor(MonthPeriod period) =>
        Path.Combine(_directory, $"{period.Start:yyyy-MM-dd}.json");
}
